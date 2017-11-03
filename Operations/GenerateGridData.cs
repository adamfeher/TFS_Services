using Microsoft.TeamFoundation.TestManagement.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RainforestExcavator
{
    public partial class Operations
    {
        /// <summary>
        /// Populates the specified grid with a list of test cases and relevant info.
        /// </summary>
        /// <param name="suite">The suite for which to search for suites</param>
        /// <param name="grid">The grid to populate</param>
        public Task<ObservableCollection<Services.TFS.TestCaseTableData>> GenerateGridData(object id, short filter)
        {
            return Task.Factory.StartNew(() =>
            {
                if (id == null) return null;

                //var testCases = suite.TestCases as ITestCaseCollection;
                ObservableCollection<Services.TFS.TestCaseTableData> testList = new ObservableCollection<Services.TFS.TestCaseTableData>();

                ITestSuiteBase suite = Session.workingProject.TestSuites.Find((int)id);

                if (suite == null) { } //Some error handling path? TOEXPAND

                //finds testpointsIds for a specific suite, used to hook testresults to a specific suite
                var testPoints = TFS.Session.workingPlan.QueryTestPoints($"SELECT * FROM TestPoint WHERE SuiteId = {id}")
                                                        .ToDictionary(t => t.TestCaseId, t => t);
                //var testPointIds = (from t in testPoints select t.Id).ToList();
            
                foreach (var tc in suite.TestCases)
                {
                    // create the table data for this tc
                    Services.TFS.TestCaseTableData tcTableData = new Services.TFS.TestCaseTableData();
                    tcTableData.Id = tc.Id;
                    tcTableData.Name = tc.Title;
                    tcTableData.Result = GetCurrentResult(testPoints[tc.Id]);

                    // find RFID if it exists
                    try
                    {
                        string rfIdTag = Parse.ExtractIdFromTags("RFID", tc.TestCase.WorkItem.Tags.Split(';'));
                        tcTableData.RFID = rfIdTag ?? "";
                    }
                    catch (InvalidOperationException) { tcTableData.RFID = "Multiple Error";}

                    // get the step count from RFID if it exists
                    if (tcTableData.RFID != "")
                    {
                        try
                        {
                            var RFtest = Services.Rainforest.Tests.GetSingle(tcTableData.RFID);
                            tcTableData.StepCount = (int)RFtest.extras["step_count"];
                            tcTableData.BrowserCount = RFtest.browsers.Count;
                        }
                        catch (Core.HttpException e)
                        {
                            // matching RFTest based on RFID no longer exists, remove RFID tag
                            this.TFS.RemoveTag(tc.Id, $"RFID{tcTableData.RFID}");
                            tcTableData.RFID = "";
                        }
                    }

                    // find if attachments exist
                    tcTableData.HasDataSeed = this.TFS.GetAttachmentIndexByName(tc.TestCase.WorkItem, "dataseed.csv") >= 0;

                    testList.Add(tcTableData);
                }
                return testList;
            });
        }

        /// <summary>
        /// Extracts the current result as string from a given Testpoint
        /// </summary>
        private string GetCurrentResult(ITestPoint tp)
        {
            // check for cases where causes are unknown
            if (tp.State == TestPointState.None || tp.State == TestPointState.InProgress || tp.State == TestPointState.MaxValue)
            {
                return "Unknown";
            }

            // if 'Ready' then cannot look at most recent result as tp is waiting for new outcome
            if (tp.State == TestPointState.Ready) { return "Active"; }

            // leftover cases where state == 'Completed' or 'NotReady', can extract most recent outcome
            return Enum.GetName(typeof(TestOutcome),
                tp.MostRecentResultOutcome);
        }
    }
}
