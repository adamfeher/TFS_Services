using RainforestExcavator.Services.Rainforest.JsonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainforestExcavator
{
    public partial class Operations
    {
        /// <summary>
        /// Based on a list of ids, this copies each TFS TC with matching id to RF
        /// </summary>
        /// <param name="testCaseIds">List of TFS testcase IDs.</param>
        /// <param name="browsers">List of browser names as defined by RF.</param>
        /// <returns></returns>
        public Task<List<Core.ServiceException>> CopyTestToRainforest(List<int> testCaseIds, List<string> browsers, Site site)
        {
            return Task.Factory.StartNew(() =>
            {
                ViewModelLocator.MainWindowViewModel.TaskManager.AddTask();
                // store failures, to be returned to UI layer for displaying
                List<Core.ServiceException> failedPush = new List<Core.ServiceException>();

                foreach (int id in testCaseIds)
                {
                    // Get the required tc data from TFS
                    Services.TFS.TestCaseCopyData tfsCopyData = null;
                    try
                    {
                        tfsCopyData = TFS.GetCopyData(id, site);
                    }
                    catch (Core.ServiceException e)
                    {
                        failedPush.Add(e);
                        continue;
                    }

                    Test rfTestCase = null;

                    // extract the RFID tag from the tfs tc
                    string rfIdTag = null;
                    try
                    {
                        rfIdTag = Parse.ExtractIdFromTags("RFID", tfsCopyData.Tags);
                    }
                    catch (InvalidOperationException)
                    {
                        // somehow has too many tags and it can't be determined which is correct
                        Core.ServiceException e = new Core.ServiceException(Core.Error.TFSTestTooManyTags, id.ToString());
                        failedPush.Add(e);
                        continue;
                    }

                    // create new RF tc or update
                    if (rfIdTag == null)
                    {
                        // create new in RF
                        rfTestCase = Services.Rainforest.Tests.Create(tfsCopyData.Title);
                        TFS.AddTag(id, $"RFID{rfTestCase.id}");
                    }
                    else
                    {
                        // get existing
                        string rfID = rfIdTag;
                        try
                        {
                            rfTestCase = Services.Rainforest.Tests.GetSingle(rfID);
                        }
                        catch(Core.HttpException)
                        {
                            failedPush.Add(new Core.ServiceException(Core.Error.RFTestDoesNotExist, id.ToString()));
                        }
                    }

                    // assign title from TFS to RF in case there were updates
                    rfTestCase.title = tfsCopyData.Title;

                    // assign description from TFS to RF
                    rfTestCase.description = tfsCopyData.Description;

                    // assign the test steps from TFS testStepData to RF Elements
                    rfTestCase.elements = new List<TestElement>();
                    try
                    {
                        // redirect bool values in RF API are stored somewhat awkwardly
                        // if the step is an embedded step at the start of a test case then it is auto redirected
                        //      to its start url, but the redirect bool is set to false
                        // otherwise an embedded step will hold the bool and it will reflect the value of the
                        //      checkbox presented before the step in the UI on RF
                        // each test case also has a start url, which problematically appears as a redirect box
                        //      in the UI after ANY sized chain of embedded steps at the start of the test
                        // 
                        // the tool will look for "redirect" TAGS in TFS in order to set these redirect values in RF
                        // this means that two versions may exist for the same embedded test, one redirecting and one not
                        // the reverse logic for this issue for RF -> VSTS is in TS.TestSteps.cs
                        bool redirect = false;
                        bool haveProcessedNormalStep = false;
                        bool isFirstStep = true;

                        // replace the rfTestCase object's steps with the steps from TestCaseCopyData
                        foreach (Services.TFS.TestStepData teststep in tfsCopyData.Steps)
                        {
                            // this is awful logic by design of Rainforest
                            if(isFirstStep)
                            {
                                // by default the first step is always false
                                redirect = false;
                                isFirstStep = false;
                            }
                            else if ((!haveProcessedNormalStep) && (teststep.TFSSharedId == null))
                            {
                                // if first normal step after some number of embedded steps
                                redirect = TFS.ContainsTag(id, "redirect");
                            }
                            else if(teststep.TFSSharedId != null)
                            {
                                // if an embedded step, but not the first step
                                redirect = TFS.ContainsTag((int)teststep.TFSSharedId, "redirect");
                            }
                            else
                            {
                                // if just a normal step at some point other than the first
                                redirect = false;
                            }

                            ElementDocumentation elementDoc = new ElementDocumentation(teststep.RFEmbeddedId, teststep.Title, teststep.ExpectedResult);
                            TestElement element = new TestElement(elementDoc, redirect);
                            rfTestCase.elements.Add(element);

                            // determine if this is first normal step processed
                            haveProcessedNormalStep = haveProcessedNormalStep || (teststep.TFSSharedId == null);
                        }
                    }
                    catch (Core.ServiceException e)
                    {
                        e.AffectedId = id.ToString();
                        failedPush.Add(e);
                        continue;
                    }

                    // copy the tags from TFS data to RF tc, leave out RFID tag
                    List<string> tags = (from tag in tfsCopyData.Tags where (!tag.Contains("RFID")) select tag).ToList();
                    rfTestCase.tags = tags;
                    rfTestCase.tags.Add($"TFSID{id}");

                    // update with the provided browser set from the UI into RF Browsers (if needed)
                    rfTestCase.browsers = browsers;

                    // set the start_uri (aka the redirect url)
                    rfTestCase.start_uri = tfsCopyData.RedirectUrl;

                    // set the site to test this case on
                    rfTestCase.extras["site_id"] = (site == null) ? null : site.id;

                    var response = Services.Rainforest.Tests.UpdateSingle(rfTestCase);
                }
                ViewModelLocator.MainWindowViewModel.TaskManager.RemoveTask();
                return failedPush;
            });
        }
    }  
}
