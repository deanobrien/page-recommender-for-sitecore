using Sitecore.Data;

namespace DeanOBrien.Feature.PageRecommender.Services
{
    public interface IGoalTriggerServices
    {
        bool TriggerGoal(ID goalId, ID pageId);
    }
}
