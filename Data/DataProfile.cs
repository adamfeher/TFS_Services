using System.Collections.Generic;
using System.Linq;

namespace RainforestExcavator.Core.Data
{
    public partial class Aggregator
    {
        /// <summary>
        /// Holds relevant information pulled from the dataseeding csv for the test case currently being processed
        /// </summary>
        public class DataProfile
        {
            // true for generic, false for specific
            public bool UserTypeIsGeneric { get; }         
            public int NumExtraUsers { get; }
            // below two store original lines as pulled from dataseed file for copying
            public string OrigTabVarNames { get; set; }
            public string OrigTabVarValues { get; set; }

            // A collection of the original values pulled from the header
            public Dictionary<string, Tuple<string, bool>> HeaderDict { get; }
            // An updated copy of HeaderDict that has been injected with unqiue values as needed for RF duplication process
            public Dictionary<string, string> InjectedHeaderDict { get; set; }
            // A collection of all the users created within this DataProfile for cycle of the RF duplication process
            // users are replaced every iteration of the duplication loop
            public Dictionary<string, Dictionary<string, string>> UserDataDict { get; }

            public DataProfile(string type, int numExtraUsers)
            {
                this.UserTypeIsGeneric = (type == "generic");
                this.NumExtraUsers = numExtraUsers;
                this.HeaderDict = new Dictionary<string, Tuple<string, bool>>();
                this.UserDataDict = new Dictionary<string, Dictionary<string, string>>();
                this.InjectedHeaderDict = null;
                this.OrigTabVarNames = string.Empty;
                this.OrigTabVarValues = string.Empty;
            }

            /// <summary>
            /// Populates the user data dictionary with generated data for a generic user with given name.
            /// </summary>
            public Dictionary<string, string> MakeGenericUserDict(string userName)
            {
                // convert the username to some unique username
                string changed = userName;
                return new Dictionary<string, string>
                {
                    {"username", changed },
                    {"email", "email"}, // TODO - generate unique email mapping -> <something>@e.rainforestqa.com
                    {"password", "password"},
                    {"firstname", changed },
                    {"lastname", "LastName"}
                };
            }

            /// <summary>
            /// Populates the user data dictionary with data extracted from the tab var set, already confirmed to exist in ReadTabVars(..).
            /// </summary>
            public Dictionary<string, string> MakeSpecificUserDict()
            {
                string[] splitTabVars = this.OrigTabVarValues.Split(',');
                return new Dictionary<string, string>
                {
                    {"username", splitTabVars[0]},
                    {"email", splitTabVars[1]},
                    {"password", splitTabVars[2]},
                    {"firstname", splitTabVars[3]},
                    {"lastname", splitTabVars[4]}
                };
            }

            /// <summary>
            /// Replaces InjectedHeaderDict with a new copy of HeaderDict with inected values where needed
            /// </summary>
            /// <param name="offset">A ref to Aggregator.uniqueCounter</param>
            public void CreateUniqueInjectedCopy(ref int offset)
            {
                // req'd due to delegate below disallowing use of ref
                int offsetDelta = offset;
                var temp = new Dictionary<string, Tuple<string,bool>>(this.HeaderDict);
                this.InjectedHeaderDict = temp.ToDictionary(
                    pair => pair.Key,
                    pair => 
                    {
                        if (!pair.Value.Item2) { return pair.Value.Item1; }
                        Tuple<string, bool> newTuple = new Tuple<string, bool>(pair.Value);
                        newTuple.Item1 = newTuple.Item1.Replace("${*}", $"{offsetDelta}");
                        offsetDelta++;
                        return newTuple.Item1;
                    });
                offset = offsetDelta;
            }
        }
    }
}
