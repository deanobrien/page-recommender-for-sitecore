using DeanOBrien.XConnect.ML;
using Sitecore.Processing.Engine.ML.Abstractions;
using Sitecore.Processing.Engine.Projection;
using Sitecore.XConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeanOBrien.XConnect.Models.ProcessingEngine
{
    public class PageRecommendationModel : IModel<Interaction>
    {
        /// <summary>
        /// The name of the options key containing the name of the table to project the data into.
        /// </summary>
        public const string OptionTableName = "tableName";

        /// <summary>
        /// The name of the table to project the data into.
        /// </summary>
        private readonly string _tableName;
        private readonly IPageRecommender _machinelearning;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="options">The options used to configure the model.</param>
        public PageRecommendationModel(IReadOnlyDictionary<string, string> options, IPageRecommender machinelearning)
        {
            _tableName = options[OptionTableName];
            _machinelearning = machinelearning;
        }

        /// <summary>
        /// The projection used to transform the xConnect data.
        /// </summary>
        public IProjection<Interaction> Projection =>
            Sitecore.Processing.Engine.Projection.Projection.Of<Interaction>()
                .CreateTabular(_tableName,
                    interaction => interaction.Events.OfType<Goal>().Select(e => new { ItemId = e.ItemId, EngagementValue = e.EngagementValue, ContactId = interaction.Contact.Id }),
                    cfg => cfg
                        .Key("ItemId", x => x.ItemId)
                        .Key("ContactId", x => x.ContactId)
                        .Measure("Engagement", x => x.EngagementValue)
                );

        public Task<ModelStatistics> TrainAsync(string schemaName, CancellationToken cancellationToken, params TableDefinition[] tables)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<object>> EvaluateAsync(string schemaName, CancellationToken cancellationToken, params TableDefinition[] tables)
        {
            throw new NotImplementedException();
        }
    }
}
