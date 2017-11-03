using Microsoft.TeamFoundation.TestManagement.Client;
using RainforestExcavator.Services.Rainforest.JsonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RainforestExcavator.Services
{
    public partial class TFS
    {
        /// <summary>
        /// Used within TestCaseCopyData objects to store relevant step data for each step in a TFS test
        /// </summary>
        public class TestStepData
        {
            public int? TFSSharedId { get; set; }          // this will hold id of shared TFS step, or null if not shared
            public int? RFEmbeddedId { get; set; }          // this will hold id of shared RF step, or null if not shared
            public string Title { get; set; }
            public string ExpectedResult { get; set; }
            public TestStepData(int? tfsId, int? rfId, string title, string expectedResult)
            {
                this.TFSSharedId = tfsId;
                this.RFEmbeddedId = rfId;
                this.Title = title;
                this.ExpectedResult = expectedResult;
            }
        }

        /// <summary>
        /// Generates all the step data from a set of TFS test actions
        /// </summary>
        /// <param name="actions">Collection of steps from the tc.</param>
        /// <param name="id">TFS tc id for displaying a helpful error message.</param>
        /// <param name="site">Required for pushing embedded steps to matching Site.</param>
        /// <returns></returns>
        public List<TestStepData> GetStepData(TestActionCollection actions, int id, Site site)
        {
            List<TestStepData> stepList = new List<TestStepData>();

            foreach (ITestAction action in actions)
            {
                if (action is ISharedStepReference)
                {
                    // if action is a shared step, ensure it is pushed to RF
                    ISharedStepReference sharedRef = action as ISharedStepReference;
                    var failures =  this.Operations.CopyTestToRainforest(new List<int>{ sharedRef.SharedStepId }, null, site).Result;
                    
                    // get the RFID for the TFS SharedStep
                    int? rfId = this.GetRFID(sharedRef.SharedStepId);
                    if (rfId == null) { throw new Core.ServiceException(Core.Error.TFSSharedTestDoesNotExist, sharedRef.SharedStepId.ToString()); }
                    // add to list
                    stepList.Add(new TestStepData(sharedRef.SharedStepId, rfId, null, null));
                }
                else if (action is ITestStep)
                {
                    // action is a normal step, not shared
                    ITestStep step = action as ITestStep;
                    stepList.Add(new TestStepData(null, null, Regex.Replace(step.Title, @"<[^>]*>", ""), Regex.Replace(step.ExpectedResult, @"<[^>]*>", "")));

                    // check for empty step
                    if (stepList.Last().Title.Trim().Equals(""))
                    {
                        stepList.RemoveAt(stepList.Count - 1);
                        continue;
                    }

                    var lastExpectedResult = stepList.Last().ExpectedResult.Trim();
                    // check for empty expected result
                    if (lastExpectedResult.Equals("")) { throw new Core.ServiceException(Core.Error.EmptyExpectedResult, id.ToString()); }
                    // check for a question mark on the expected result

                    if (lastExpectedResult.Trim().Last() != '?') { stepList.Last().ExpectedResult += '?'; }

                } else { throw new Exception("TestCase action is unknown type."); } // TOEX -> this case shouldn't be hit ever
            }
            return stepList;
        }
    }
}
