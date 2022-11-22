using DeanOBrien.Feature.PageRecommender.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

namespace DeanOBrien.Feature.PageRecommender.Configurator
{
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IGoalTriggerServices, GoalTriggerServices>();
        }
    }
}
