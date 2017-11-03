using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RainforestExcavator.Core.Data
{
    public partial class Aggregator
    {
        /// <summary>
        /// Writes out a single generic user to the pool. 
        /// </summary>
        /// <param name="outStream">The stream for the generic user tab var csv.</param>
        /// <param name="dataProfile">Reference to the current DataProfile instance.</param>
        private void GenerateGenericUser()
        {
            using (StreamWriter outStream = new StreamWriter(Core.Filepaths.GenericOutFile, true))
            {
                // generate the user data
                Dictionary<string, string> userData = this.dataProfile.MakeGenericUserDict($"{this.projectInfoPair.Value}User{genericPoolCounter}");
                // extract the values and join for writing out to csv
                string outLine = string.Join(",", userData.Select(lookup => { return lookup.Value; }));
                outStream.WriteLine(outLine);
                // store for later use and generate extra users
                this.dataProfile.UserDataDict["main"] = userData;
                GenerateExtraUsers($"{genericPoolCounter}");
            }
        }

        private void GenerateSpecificUser(string tfsID)
        {
            // extract the user data and create extra users
            this.dataProfile.UserDataDict["main"] = this.dataProfile.MakeSpecificUserDict();
            GenerateExtraUsers(tfsID);
        }

        private void GenerateExtraUsers(string nameSuffix)
        {
            if (this.dataProfile.NumExtraUsers == 0) { return; }

            // extrausers start at 1 -> less confusing for tc writers
            for (int i = 1; i <= this.dataProfile.NumExtraUsers; i++)
            {
                // assign a char suffix for extra users to appear as siblings, supports up to 26 nicely, should be more than enough
                char siblingId = (char)(65 + (i % this.dataProfile.NumExtraUsers));
                Dictionary<string, string> userData = this.dataProfile.MakeGenericUserDict($"{this.projectInfoPair.Value}ExtraUser{nameSuffix + siblingId}");
                this.dataProfile.UserDataDict[$"extra{i}"] = userData;
            }
        }
    }
}
