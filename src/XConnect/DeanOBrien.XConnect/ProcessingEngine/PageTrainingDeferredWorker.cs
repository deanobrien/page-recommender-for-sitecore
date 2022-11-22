using DeanOBrien.XConnect.ML;
using DeanOBrien.XConnect.Models;
using Microsoft.Extensions.Logging;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Engine.Storage.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeanOBrien.XConnect.ProcessingEngine
{
    public class PageTrainingDeferredWorker : IDeferredWorker
    {
        private readonly IPageRecommender _pageRecommender;
        private readonly ILogger _logger;
        public const string OptionSourceTableName = "sourceTableName";

        public const string OptionTargetTableName = "targetTableName";

        public const string OptionSchemaName = "schemaName";

        public const string OptionLimit = "limit";

        private readonly ITableStore _tableStore = null;

        private readonly string _sourceTableName = null;

        private readonly string _targetTableName = null;

        private readonly int _limit = 1;

        public PageTrainingDeferredWorker(
            IPageRecommender pageRecommender,
            ITableStoreFactory tableStoreFactory,
            IReadOnlyDictionary<string, string> options,
            ILogger logger)
        {
            _pageRecommender = pageRecommender;
            _logger = logger;
            _sourceTableName = options[OptionSourceTableName];
            _targetTableName = options[OptionTargetTableName];
            _limit = int.Parse(options[OptionLimit]);

            var schemaName = options[OptionSchemaName];
            _tableStore = tableStoreFactory.Create(schemaName);
        }

        public void Dispose()
        {
            _tableStore.Dispose();
        }

        public async Task RunAsync(CancellationToken token)
        {
            var sourceRows = await _tableStore.GetRowsAsync(_sourceTableName, CancellationToken.None);

            var data = new List<PageEngagement>();

            while (await sourceRows.MoveNext())
            {
                foreach (var row in sourceRows.Current)
                {
                    var pageEngagement = new PageEngagement() { ContactId = row["ContactId"].ToString(), PageId = row["ItemId"].ToString(), Engagement = float.Parse(row["Engagement"].ToString()) };
                    data.Add(pageEngagement);
                }
            }
            _pageRecommender.Train(data);
        }
    }
}
