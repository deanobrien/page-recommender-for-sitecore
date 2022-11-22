using Sitecore.Analytics;
using Sitecore.Analytics.Data;
using Sitecore.Data;
using Sitecore.Data.Items;
using System;


namespace DeanOBrien.Feature.PageRecommender.Services
{
    public class GoalTriggerServices : IGoalTriggerServices
    {
        public bool TriggerGoal(ID goalId, ID pageId)
        {
            if (Tracker.Enabled)
            {
                if (!Tracker.IsActive) Tracker.StartTracking();

                var page = Tracker.Current.Session.Interaction?.CurrentPage;
                if (page != null)
                {
                    Item goalItem = Sitecore.Context.Database.GetItem(goalId);

                    var pageEventData = new PageEventData("", new Guid(goalItem.ID.ToString()))
                    {
                        ItemId = new Guid(pageId.ToString()),
                        Data = "data field from goal GoalTriggerServices.cs",
                        Text = "text field from goal GoalTriggerServices.cs"
                    };
                    Sitecore.Analytics.Tracker.Current.Interaction.CurrentPage.Register(pageEventData);
                    return true;
                }
            }
            return false;
        }
    }
}
