using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RainforestExcavator.Services
{
    public partial class TFS
    {
        /// <summary>
        /// Adds the provided tag to the TestCase or SharedStep with the provided TFSID.
        /// </summary>
        /// <returns>Bool indicating success.</returns>
        public bool AddTag(int tfsId, string tag)
        {
            // Get test case from id, it might be a set of shared steps
            WorkItem workItem = Session.workingProject.WitProject.Store.GetWorkItem(tfsId);
            if (workItem == null) { return false; }
            workItem.Tags += ";" + tag;
            workItem.Save();
            return true;
        }
        /// <summary>
        /// Removes the provided tag from the TestCase or SharedStep with provided TFSID.
        /// </summary>
        /// /// <returns>Bool indicating success.</returns>
        public bool RemoveTag(int tfsId, string tag)
        {
            WorkItem workItem = Session.workingProject.WitProject.Store.GetWorkItem(tfsId);
            if (workItem == null) { return false; }
            List<string> tags = workItem.Tags.Split(';').Select(t => t.Trim()).ToList();
            bool success = tags.Remove(tag);
            if(success)
            {
                workItem.Tags = string.Join(";", tags);
                workItem.Save();
            }
            return success;
        }
        /// <summary>
        /// Searches for the RFID tag on the TestCase or SharedStep with the provided TFSID
        /// </summary>
        /// <returns>Nullable int representing RFID if any.</returns>
        public int? GetRFID(int tfsId)
        {
            // Get test case from id, it might be a set of shared steps
            ITestBase testCase = Session.workingProject.TestCases.Find(tfsId);
            testCase = testCase ?? this.Session.workingProject.SharedSteps.Find(tfsId);

            try
            {
                string rfIdTag = Parse.ExtractIdFromTags("RFID", testCase.WorkItem.Tags.Split(';'));
                if (rfIdTag == null) { return null; }
                return Int32.Parse(rfIdTag);
            }
            catch (InvalidOperationException) { throw new Core.ServiceException(Core.Error.TFSTestTooManyTags); }
        }
        /// <summary>
        /// Returns whether or not the TestCase or SharedStep is tagged with the provided tag
        /// </summary>
        public bool ContainsTag(int tfsId, string tag)
        {
            WorkItem workItem = Session.workingProject.WitProject.Store.GetWorkItem(tfsId);

            if (workItem == null) { return false; }
            var foundTags = (from t in workItem.Tags.Split(';') where t.Contains(tag) select t).ToList();
            if (foundTags.Count == 0) { return false; }
            else { return true; }
        }
    }
}
