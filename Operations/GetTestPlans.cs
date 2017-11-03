using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainforestExcavator
{
    public partial class Operations
    {
        /// <summary>
        /// Gets the list of test plans under the selected project.
        /// </summary>
        public Task<List<ITestPlan>> GetTestPlans(Project currentProjectRef)
        {
            return Task.Factory.StartNew(() =>
            {
                List<ITestPlan> planList = new List<ITestPlan>();
                this.Session.workingProject = Session.TestService.GetTeamProject(currentProjectRef.Name);
                ITestPlanCollection plans = Session.workingProject.TestPlans.Query("SELECT * FROM TestPlan Order By PlanName");
                return plans.ToList();
            });
        }
    }
}
