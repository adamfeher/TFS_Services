using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RainforestExcavator
{
    public partial class Operations
    {
        /// <summary>
        /// Gets a list of projects for the user.
        /// </summary>
        public Task<List<Project>> GetProjects()
        {
            return Task.Factory.StartNew(() =>
            {
                //Retrieve the user's project list
                WorkItemStore workItems = new WorkItemStore(Session.ProjectCollection);
                ProjectCollection projects = workItems.Projects;

                var projectList = new List<Project>();
                foreach (Project p in projects)
                {
                    // try to list test plans for each project, if none are returned then user does not have access
                    var proj = Session.TestService.GetTeamProject(p.Name);
                    ITestPlanCollection plans = proj.TestPlans.Query("SELECT * FROM TestPlan");
                    if (plans.Count != 0)
                    {
                        projectList.Add(p);
                    }
                }
                return projectList;
            });
        }
    }
}
