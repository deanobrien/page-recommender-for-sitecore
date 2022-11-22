using Sitecore.Data.Items;
using System.Linq;
using System;
using System.Collections.Generic;


namespace DeanOBrien.Feature.PageRecommender.Tasks
{
    public class GeneratePageRecommendationsTask
    {
        public void Execute(Item[] items, Sitecore.Tasks.CommandItem command, Sitecore.Tasks.ScheduleItem schedule)
        {
            Sitecore.Diagnostics.Log.Info("Generate Page Recommendations Task: Started", this);
            try
            {

                var generatePageRecommendationsTask = new GeneratePageRecommendations();
                generatePageRecommendationsTask.Run(items, "GeneratePageRecommendationsTask Sitecore Scheduled Task");
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("Generate Page Recommendations Task error: " + ex.Message, this);
            }
        }
    }
}