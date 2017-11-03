using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RainforestExcavator.Core.Data
{
    public static class Process
    {
        /// <summary>
        /// Searches a line of dataseeding text for custom formatted keywords and inserts the respective values from the provided dict.
        /// </summary>
        /// Designed custom keywords follow the following format : ${x} where 'x' is a string
        public static string LineInjectHeaderValues(string line, Aggregator.DataProfile dataprofile)
        {
            char[] lineOriginal = line.ToCharArray();
            int lineLength = lineOriginal.Length;
            StringBuilder stringBuilder = new StringBuilder();

            // read through each char looking for formatted keywords
            for (int index = 0; index < lineLength - 1; index++)
            {
                char current = lineOriginal[index];
                char next = lineOriginal[index + 1];

                // check for beginning of keyword
                if (current == '$' && next == '{')
                {
                    int indexEndBrace = line.IndexOf('}', index + 2);

                    // check for missing end brace on last tab var
                    if (indexEndBrace == -1) { throw new ServiceException(Error.InvalidTabVarFormatting);}

                    // pull out the keyword, remove case sensitivity
                    string keyword = line.Substring(index + 2, indexEndBrace - index - 2).ToLower();

                    // check for missing end brace for a tab var that is not last
                    if (keyword.Contains("{")) { throw new ServiceException(Error.InvalidTabVarFormatting); }


                    if (keyword.Contains("."))
                    {
                        // if used for a non-Generic user, this is invalid
                        if (!dataprofile.UserTypeIsGeneric) { throw new ServiceException(Error.InvalidUserVarUse); }

                        // check whether variable is for a generated user value (ie -> ${user.email} )
                        string[] keywordSplit = keyword.Split('.');

                        // until there are more variables in dataseed files that are of format ${x.y}
                        // this case requires a lookup of "main" user
                        try { stringBuilder.Append((dataprofile.UserDataDict[keywordSplit[0]])[keywordSplit[1]]); }
                        catch (KeyNotFoundException) { throw new ServiceException(Error.InvalidUserVarUse); }
                    }
                    else if (dataprofile.InjectedHeaderDict.ContainsKey(keyword))
                    {
                        // find the replacement value for the keyword
                        stringBuilder.Append(dataprofile.InjectedHeaderDict[keyword]); 
                    }
                    else { throw new ServiceException(Error.HeaderVarDoesNotExist); }
                    index += keyword.Length + 2;        // word length plus braces
                }
                else { stringBuilder.Append(current); }
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Reads through a stream of a csv and returns the first non-empty line it finds
        /// </summary>
        /// <param name="inStream"></param>
        /// <returns></returns>
        public static string GetFirstLine(StreamReader inStream)
        {
            string line = string.Empty;
            try {
                while (!inStream.EndOfStream)
                {
                    //find first line with values
                    line = inStream.ReadLine();
                    if (line.Trim(',').Trim() != string.Empty) { break; }
                }
            }
            catch(Exception e)
            {
                var a = 5;
            }
            return line;
        }
    }
}
