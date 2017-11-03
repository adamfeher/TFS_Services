using System;
using System.IO;
using System.Linq;

namespace RainforestExcavator.Core.Data
{
    public partial class Aggregator
    {
        /// <summary>
        /// Retrieves the header from the dataseed file and creates a new DataProfile with extracted values.
        /// </summary>
        /// <param name="inStream">Dataseeding file's input stream.</param>
        /// <returns>Returns false if dataseed is empty, else true.</returns>
        private bool ParseHeader(StreamReader inStream)
        {
            string firstLine = Process.GetFirstLine(inStream);
            // check for empty data seed case
            if (firstLine == string.Empty) { return false; }

            string[] headerNames = firstLine.Split(',').Where(name => name != string.Empty).ToArray();
            string[] headerValues = inStream.ReadLine().Split(',').Where(value => value != string.Empty).ToArray();
            
            // incorrect header formatting check
            if (headerNames.Length != headerValues.Length) { throw new ServiceException(Error.HeaderLengthMismatch); }
            if ((headerNames.Length < 2) || (headerValues.Length < 2)) { throw new ServiceException(Error.HeaderTooShort); }
            if ((headerNames[0].ToLower() != "usertype") || (headerNames[1].ToLower() != "numextrausers")) 
                { throw new ServiceException(Error.HeaderTooShort); }
            
            
            // extract the UserType and number of extra users required
            string userType = headerValues[0].ToLower();
            int numExtraUsers;
            try
            {
                numExtraUsers = Int32.Parse(headerValues[1]);
            }
            catch(FormatException) { throw new ServiceException(Error.InvalidExtraUserCount); }

            //Check if usertype is undetermined
            if ((userType != "generic") && (userType != "specific")) { throw new ServiceException(Error.InvalidUserType); }
            
            this.dataProfile = new DataProfile(userType, numExtraUsers);

            for (int index = 2; index < headerNames.Length; index++)
            {
                // check for trailing commas
                if ((headerNames[index] == string.Empty) || (headerValues[index] == string.Empty)) { continue; }
                // check for unique number indicator in value
                bool containsUniqueIndicator = headerValues[index].Contains("${*}");

                this.dataProfile.HeaderDict.Add(headerNames[index].Trim().ToLower(),
                    new Data.Tuple<string, bool>(headerValues[index].Trim().ToLower(), containsUniqueIndicator));
            }
            return true;
        }
    }
}
