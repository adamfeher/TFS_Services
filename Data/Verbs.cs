using System.Collections.Generic;
using System.IO;

namespace RainforestExcavator.Core.Data
{
    public partial class Aggregator
    {
        private void ParseCommands(StreamReader inStream, string tfsID)
        {
            using (StreamWriter outStream = new StreamWriter(Core.Filepaths.VerbOutFile, true))
            {
                // read remaining lines into a list so that it can be looped over for duplication purposes without opening new streams
                List<string> commandLines = new List<string>();
                while (!inStream.EndOfStream)
                {
                    string readLine = inStream.ReadLine();
                    if (readLine.Trim().Trim(',') == string.Empty) { continue; }
                    commandLines.Add(readLine);
                }

                // TODO
                // Validate each commandLine
                // Here is where each commandLine should be validated to ensure it has correct # of params with correct types
                // The point is to catch an error at this stage during data compilation instead of deployment where it will
                // take more time and effort to fix any dataseeding errors.
                //
                // Original idea was to store "contracts" in the Code base of each Project in TFS for each dataseeding script
                // At this point the "contracts" would be downloaded and stored in a quick lookup type.
                // Then each commandLine would find its matching "contract" to be validated against, throwing any errors found


                // assign dbID and name
                // if generic then productID used (placeholder, could be changed), else tfsID used
                string dbId = (this.dataProfile.UserTypeIsGeneric) ? this.projectInfoPair.Key.ToString() : $"{tfsID}{1}";       // TODO this needs to be unique-ish
                string dbName = (this.dataProfile.UserTypeIsGeneric) ? $"generic{this.projectInfoPair.Key}" : $"{tfsID}_{1}";

                // Initiate required config, subscription, and user for this test
                Dictionary<string, string> userDict = this.dataProfile.UserDataDict["main"];
                outStream.WriteLine($"SetConfig.ps1 ${this.dataProfile.UserTypeIsGeneric.ToString().ToLower()} {dbId} {dbName}");
                outStream.WriteLine($"UserSetup.ps1 {userDict["firstname"]} {userDict["lastname"]} {userDict["email"]}");

                // extra users start at 1 -> less confusing for tc writers
                for (int i = 1; i <= this.dataProfile.NumExtraUsers; i++)
                {
                    userDict = this.dataProfile.UserDataDict[$"extra{i}"];
                    outStream.WriteLine($"UserSetup.ps1 {userDict["firstname"]} {userDict["lastname"]} {userDict["email"]} -extra");
                }
                // iterate over read in lines, inject values, write to file
                commandLines.ForEach(line =>
                {
                    // inject the line with values from the header
                    string injectedLine = Process.LineInjectHeaderValues(line, this.dataProfile);

                    // TODO any other formatting to pass off 
                    // may not be any once formmatting decisions are in
                    // still need to remove any commas though as each line is possible csv
                    outStream.WriteLine(injectedLine);
                });
                outStream.Flush();
            }  
        }
    }
}
