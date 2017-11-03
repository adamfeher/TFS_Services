using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Services.TFS
{
    public class Session
    {
        /// <summary>
        /// Default directory path given from AppDomain. 
        /// </summary>
        /// Placed here for now for sample
        public static readonly string BaseDir = System.AppDomain.CurrentDomain.BaseDirectory;

        public static TeamFoundationIdentity userTFId { get; set; }
        public static VssConnection VssConnection { get; set; }
        public static WorkItemTrackingHttpClient Wit { get; set; }
        public static TfsTeamProjectCollection ProjectCollection { get; set; }
        public static ITestManagementService TestService { get; set; }
        private static string _vstsAccessToken = null;

        /// <summary>
        /// Attempts to authenticate with given access token, returns bool indicating success
        /// </summary>
        public static bool VSTSConnect(string accessToken)
        {
            VssBasicCredential vssCreds = new VssBasicCredential(string.Empty, accessToken);
            Task connectTask = null;
            try
            {
                VssConnection = new VssConnection(new Uri(@"https://adamfeher.visualstudio.com"), vssCreds);
                connectTask = VssConnection.ConnectAsync();
                Session.ProjectCollection = new TfsTeamProjectCollection(new Uri(@"https://adamfeher.visualstudio.com/DefaultCollection"), vssCreds);
                Session.ProjectCollection.Authenticate();
            }
            catch (Exception e) { return false; }

            if (!connectTask.IsCompleted) { connectTask.SyncResult(); }

            Wit = VssConnection.GetClient<WorkItemTrackingHttpClient>();
            TestService = ProjectCollection.GetService<ITestManagementService>();

            return true;
        }

        /// <summary>
        /// Disconnects the current TFS session
        /// </summary>
        public static bool VSTSDisconnect()
        {
            Session.ProjectCollection.Dispose();
            Session.ProjectCollection = null;
            Session.TestService = null;
            return (Session.ProjectCollection == null);
        }
        /// <summary>
        /// Returns bool indicating the current connection status of TFS
        /// </summary>
        /// <returns></returns>
        public static bool IsVSTSConnected()
        {
            return (VssConnection != null) && VssConnection.HasAuthenticated;
        }

        /// <summary>
        /// Returns the currently stored TFS client token for the user, if any
        /// </summary>
        public static string GetVSTSAcessToken()
        {
            return GetToken("vststs.txt", ref _vstsAccessToken);
        }
        /// <summary>
        /// Overwrites the VSTS token stored in the file and stores the new token into the private field
        /// </summary>
        /// <param name="token"></param>
        public static void ReplaceVSTSToken(string token)
        {
            ReplaceToken(token, "vststs.txt", ref _vstsAccessToken);
        }

        /// <summary>
        /// Overwrites the token stored in the file and stores the new token into the private field
        /// </summary>
        /// <param name="token"></param>
        private static string GetToken(string filename, ref string privTokenField)
        {
            if (privTokenField == null)
            {
                // if token hasn't been set, try to retrieve from the file
                string filePath = $"{BaseDir}{filename}";
                try
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string onlyLine = reader.ReadLine();
                        privTokenField = onlyLine.Trim();
                    }
                }
                // if file dne because this is first time running then create with the first ever working token
                catch (FileNotFoundException)
                {
                    ReplaceToken("None found, please provide new token.", filename, ref privTokenField);
                    return "None found, please provide new token.";
                }
            }
            return privTokenField;
        }
        /// <summary>
        /// Overwrites the token stored in the file and stores the new token into the private field
        /// </summary>
        private static void ReplaceToken(string token, string filename, ref string privTokenField)
        {
            string filePath = $"{BaseDir}{filename}";
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine(token);
            }
            privTokenField = token;
        }

        //public static bool SecurityCheck()
        //{
        //    IdentityHttpClient identityClient = VssConnection.GetClient<IdentityHttpClient>();

        //    IIdentityManagementService ims = Session.ProjectCollection.GetService<IIdentityManagementService>();
        //    Session.userTFId = ims.ReadIdentity(IdentitySearchFactor.Identifier, VssConnection.AuthorizedIdentity.Descriptor.Identifier, MembershipQuery.Direct, ReadIdentityOptions.None);

        //    // get the group (RainForest) and all its members
        //    Identity rfGroup = identityClient.ListGroupsAsync().SyncResult().Find(x => x.DisplayName == "[adamfeher]\\GroupName");
        //    Identity rfMembers = identityClient
        //                        .ReadIdentitiesAsync(new List<Microsoft.VisualStudio.Services.Identity.IdentityDescriptor> { rfGroup.Descriptor }, QueryMembership.Expanded)
        //                        .SyncResult()
        //                        .Single();

        //    // search the security group members to see if logged in user is member
        //    return rfMembers.Members.Any(m => m.Identifier.Contains(VssConnection.AuthorizedIdentity.Descriptor.Identifier));
        //}
    }
}