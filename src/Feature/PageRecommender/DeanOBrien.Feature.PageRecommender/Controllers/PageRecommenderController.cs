using DeanOBrien.Feature.PageRecommender.Services;
using Sitecore.Data;
using Sitecore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DeanOBrien.Feature.PageRecommender.Controllers
{
    public class PageRecommenderController : SitecoreController
    {
        // Goal Definition Ids
        private static readonly Guid Tab0 = new Guid("38470D84-BA4E-4ABC-8740-9BCD66EF6E0F");
        private static readonly Guid Tab1 = new Guid("AB7FCA23-FB0F-45D3-AB3C-7C025D2CF01F");
        private static readonly Guid Tab2 = new Guid("0F049C02-0ED7-452C-905F-C15EF2B92680");
        private static readonly Guid Tab3 = new Guid("518C5ED4-B27A-4110-B1FB-C3646E9BF8A7");
        private static readonly Guid Tab4 = new Guid("F7DE34FA-251A-4ED7-B977-7010B634884A");
        private static readonly Guid Tab5 = new Guid("0E52F41D-771F-4A3D-B1FF-60DA7B87511E");
        private static readonly Guid Tab6 = new Guid("D1C9CEC3-50AE-4076-9434-B7AE9FDDDD16");
        private static readonly List<Guid> _eventsList = new List<Guid>() { Tab0, Tab1, Tab3, Tab4, Tab5, Tab6 };

        private readonly IGoalTriggerServices _goalTriggerServices;

        public PageRecommenderController(IGoalTriggerServices goalTriggerServices)
        {
            _goalTriggerServices = goalTriggerServices;
        }
        public ActionResult Index()
        {
            return View();
        }
        public bool RegisterGoalByNameAndPageId(string goalName, string pageId)
        {
            ID goalId = FindGoalIdFromName(goalName);
            if (goalId.IsNull) return false;
            if (_goalTriggerServices.TriggerGoal(goalId, new ID(pageId))) return true;
            return false;
        }
        private ID FindGoalIdFromName(string goalName)
        {
            switch (goalName)
            {
                case "Tab0":
                    return new ID(Tab0);
                case "Tab1":
                    return new ID(Tab1);
                case "Tab2":
                    return new ID(Tab2);
                case "Tab3":
                    return new ID(Tab3);
                case "Tab4":
                    return new ID(Tab4);
                case "Tab5":
                    return new ID(Tab5);
                case "Tab6":
                    return new ID(Tab6);
                default:
                    return null;
            }
        }
    }
}
