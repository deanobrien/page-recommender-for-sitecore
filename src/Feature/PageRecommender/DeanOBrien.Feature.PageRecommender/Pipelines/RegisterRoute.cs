using Sitecore.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace DeanOBrien.Feature.PageRecommender.Pipelines
{
    public class RegisterRoute
    {
        public virtual void Process(PipelineArgs args)
        {
            Register();
        }

        public static void Register()
        {
            RouteTable.Routes.MapRoute("RegisterGoalByNameAndPageId", "Trigger/Goal/{goalName}/{pageId}",
                new { controller = "PageRecommender", action = "RegisterGoalByNameAndPageId" }
                );
        }
    }
}
