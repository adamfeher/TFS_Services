using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using RainforestExcavator.Services.Rainforest.JsonObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RainforestExcavator.Services
{
    public partial class TFS
    { 
        /// <summary>
        /// Used to store the relevant info for each TFS test displayed in the test case grid
        /// </summary>
        public class TestCaseTableData
        {
            public string Result { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string RFID { get; set; }
            public bool HasDataSeed { get; set; }
            public int StepCount { get; set; }
            public int BrowserCount { get; set; }
        }
        /// <summary>
        /// Used to store the relevant info for each TFS test getting copied to RF
        /// </summary>
        public class TestCaseCopyData
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public List<string> Tags { get; set; }
            public List<TestStepData> Steps { get; set; }
            public string RedirectUrl { get; set; }
            public TestCaseCopyData(string title, string description, List<string> tags, List<TestStepData> steps, string redirectUrl)
            {
                this.Title = title;
                this.Description = description;
                this.Tags = tags;
                this.Steps = steps;

                // workaround to reset redirect url from TFS UI -> must enter "empty" into history discussion
                string temp = redirectUrl.Trim();
                if (temp.ToLower().Equals("empty")) { temp = ""; }
                this.RedirectUrl = temp;
            }
        }
        /// <summary>
        /// Retrives all the relevant info required to copy a test from TFS to RF
        /// </summary>
        /// <param name="id">TFS tc id.</param>
        /// <param name="site">Required for pushing embedded steps to matching Site.</param>
        /// <returns></returns>
        public TestCaseCopyData GetCopyData(int id, Site site)
        {
            // Get test case from id, it might be a set of shared steps
            ITestBase testCase = Session.workingProject.TestCases.Find(id);

            if (testCase == null) { testCase = Session.workingProject.SharedSteps.Find(id); }
            if (testCase == null) { return null; } // this test case dne anymore
           
            List<TestStepData> stepList = GetStepData(testCase.Actions, id, site);
            List<string> tagList = testCase.WorkItem.Tags.Split(';').ToList();
            string redirectUrl = testCase.CustomFields["History"].OriginalValue.ToString();
            return new TestCaseCopyData(testCase.Title, testCase.CustomFields["Description"].Value.ToString(), tagList, stepList, redirectUrl);
        }
    }
}
