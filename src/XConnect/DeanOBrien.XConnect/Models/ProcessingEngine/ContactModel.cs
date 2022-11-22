using DeanOBrien.XConnect.ML;
using Sitecore.Processing.Engine.ML.Abstractions;
using Sitecore.Processing.Engine.Projection;
using Sitecore.XConnect;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeanOBrien.XConnect.Models.ProcessingEngine
{
    public class ContactModel : IModel<Interaction>
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
        public ContactModel(IReadOnlyDictionary<string, string> options, IPageRecommender machinelearning)
        {
            _tableName = options[OptionTableName];
            _machinelearning = machinelearning;
        }

        public IProjection<Interaction> Projection =>
                Sitecore.Processing.Engine.Projection.Projection.Of<Interaction>()
                    .CreateTabular(_tableName,
                        interaction => new { ContactId = interaction.Contact.Id },
                        cfg => cfg
                            .Key("ContactId", x => x.ContactId)
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
