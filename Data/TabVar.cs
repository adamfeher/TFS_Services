using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RainforestExcavator.Core.Data
{
    public partial class Aggregator
    {
        /// <summary>
        /// Reads in the tab vars as single csv string and stores them in the DataProfile without any modifications
        /// </summary>
        /// <param name="inStream">Dataseeding file's input stream.</param>
        private void ReadTabVars(StreamReader inStream)
        {
            // get the next line (if it exists) which marks the start of the tab var section
            this.dataProfile.OrigTabVarNames = Process.GetFirstLine(inStream);
            // return if empty tab variables and no extra users
            if (this.dataProfile.OrigTabVarNames == string.Empty) { return; }
            // read in the second line
            this.dataProfile.OrigTabVarValues = inStream.ReadLine();
            // check if second line is empty or missing
            if ((this.dataProfile.OrigTabVarValues == null)
                ||(this.dataProfile.OrigTabVarValues.Trim(',').Trim() == string.Empty))
                { throw new ServiceException(Error.TabVarValuesMissing); }

            var tempNames = this.dataProfile.OrigTabVarNames.Split(',').Where(name => name != string.Empty).ToArray();
            var tempValues = this.dataProfile.OrigTabVarValues.Split(',').Where(value => value != string.Empty).ToArray();
            this.dataProfile.OrigTabVarNames = string.Join(",", tempNames);
            this.dataProfile.OrigTabVarValues = string.Join(",", tempValues);

            // check if the tab var names and values do not have same lengths
            if (tempNames.Length != tempValues.Length)
            { throw new ServiceException(Error.TabVarLegthMismatch); }

            // if Specific User, tab var must contain info for it, check for req'd fields
            if (!this.dataProfile.UserTypeIsGeneric)
            {
                // convention for specific user was to have certain values at beginning of tab var
                // can look directly at expected index rather than doing a Contains(..)
                if (tempNames[0].ToLower() != "username" ||
                    tempNames[1].ToLower() != "email" ||
                    tempNames[2].ToLower() != "password" ||
                    tempNames[3].ToLower() != "firstname" ||
                    tempNames[4].ToLower() != "lastname")
                {
                    throw new ServiceException(Error.InvalidSpecificUserValues);
                }
            }
        }
        /// <summary>
        /// Given the filepath
        /// </summary>
        private void WriteTabVars(string outPath)
        {
            using (StreamWriter outStream = new StreamWriter(outPath, true))
            {
                string varInjectedValues = Process.LineInjectHeaderValues(this.dataProfile.OrigTabVarValues, this.dataProfile);

                // concat the extra user values into the tab var set
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(varInjectedValues);
                // extra users start at 1 -> less confusing for tc writers
                for (int i = 1; i <= this.dataProfile.NumExtraUsers; i++)
                {
                    if (stringBuilder.Length > 0) { stringBuilder.Append(","); }
                    Dictionary<string,string> lookup = this.dataProfile.UserDataDict[$"extra{i}"];
                    stringBuilder.Append($"{lookup["username"]},{lookup["email"]},{lookup["password"]},{lookup["firstname"]},{lookup["lastname"]}");
                }
                outStream.WriteLine(stringBuilder.ToString());
            }
        }

        private void WriteTabVarHeader(string outPath)
        {
            // need to concat extra user header names in as well, one set for each extra user
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(this.dataProfile.OrigTabVarNames);
            // extra users start at 1 -> less confusing for tc writers
            for (int i = 1; i <= this.dataProfile.NumExtraUsers; i++)
            {
                if (stringBuilder.Length > 0) { stringBuilder.Append(","); }
                stringBuilder.Append($"extrauser{i}username,extrauser{i}email,extrauser{i}password,extrauser{i}firstname,extrauser{i}lastname");
            }
            using (StreamWriter outStream = new StreamWriter(outPath, false))
            {
                outStream.WriteLine(stringBuilder.ToString());
            }
        }
    }
}
