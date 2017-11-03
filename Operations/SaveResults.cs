using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.TeamFoundation.TestManagement.Client;
using System;
using System.Collections.Generic;
using RainforestExcavator.Services.Rainforest.JsonObjects;

namespace RainforestExcavator
{
    public partial class Operations
    {
        public Task SaveResults(string runId, int suiteId)
        {
            return Task.Factory.StartNew(() =>
            { 
                // tell the spinner this task is starting
                ViewModelLocator.MainWindowViewModel.TaskManager.AddTask();

                // retrieve the info for the specified run
                RunInfo rfRunInfo = Services.Rainforest.Runs.GetSingle(runId);

                // check to see if the run is finished
                if (rfRunInfo.state != "complete" && rfRunInfo.state != "aborted")
                {
                    ViewModelLocator.MainWindowViewModel.TaskManager.RemoveTask();
                    throw new Core.ServiceException(Core.Error.RunNotComplete, runId);
                }

                // run has completed or has been aborted and is being processed, tag can be removed
                this.TFS.RemoveTag(suiteId, $"RUNID{runId}");

                // if run was aborted then drop results
                if (rfRunInfo.state == "aborted")
                {
                    ViewModelLocator.MainWindowViewModel.TaskManager.RemoveTask();
                    throw new Core.ServiceException(Core.Error.RunAborted, runId);
                }

                // Run was completed successfully
                // deserialize the RF tests info that were in the run, ids here are all rfIds, these are NOT JsonObjects.Test objects
                var anon = new[] { new { id = 0, result = "" } };
                var rfTestResults = JsonConvert.DeserializeAnonymousType(rfRunInfo.extras["tests"].ToString(), anon);

                // Get all the testpoints from the suite
                var testPointDict = TFS.Session.workingPlan
                                            .QueryTestPoints(String.Format("SELECT * FROM TestPoint WHERE SuiteId = {0}", suiteId))
                                            .ToDictionary(tp => tp.TestCaseId, tp => tp);

                // create a run in tfs to create results against
                ITestRun tfsTestRun = Session.workingPlan.CreateTestRun(false);

                // map the RF test results to a new enumerable with stored TFS id
                var refinedRFTestResults = rfTestResults.Select(r =>
                {
                    // retrieve the test from RF, need to search tag for matching TFS id
                    // each should have a tag already as it was ensured before the run was created
                    // TOEXPAND -> could possible add redundant check for no tags found in case manually removed
                    Test rfTest = Services.Rainforest.Tests.GetSingle(r.id.ToString());
                    string tfsId = Parse.ExtractIdFromTags("TFSID", rfTest.tags);
                    return new { rfid = r.id, verdict = r.result, tfsId = Int32.Parse(tfsId)};
                })
                .ToList();

                // go through RF results once to find the needed testpoints and add to run
                foreach (var result in refinedRFTestResults)
                {
                    // find test point for the matching TFS tc and add to run
                    ITestPoint testPoint = testPointDict[result.tfsId];
                    tfsTestRun.AddTestPoint(testPoint, testPoint.TestCaseWorkItem.Owner);
                }
                tfsTestRun.Save();

                // retrieve all the TFS results from the run
                Dictionary<int, ITestCaseResult> tfsTestResults = tfsTestRun.QueryResults().ToDictionary(r => r.TestCaseId, r => r);

                // go through RF results again to save the outcomes to the tfs results
                foreach (var result in refinedRFTestResults)
                {
                    ITestCaseResult tfsResult = tfsTestResults[result.tfsId];

                    switch (result.verdict)
                    {
                        case "passed":
                            tfsResult.Outcome = TestOutcome.Passed;
                            tfsResult.State = TestResultState.Completed;
                            break;
                        case "failed":
                            tfsResult.Outcome = TestOutcome.Failed;
                            tfsResult.State = TestResultState.Completed;
                            break;
                        default:
                            // TOEXPAND -> should never get here, verdict will always have a value
                            break;
                    }
                    tfsResult.State = TestResultState.Completed;
                    tfsResult.RunBy = Session.userTFId;
                    tfsResult.Save();
                }
                tfsTestRun.Save();
                tfsTestRun.Refresh();

                // tell the spinner this task is done
                ViewModelLocator.MainWindowViewModel.TaskManager.RemoveTask();
            });
        }
    }
}
