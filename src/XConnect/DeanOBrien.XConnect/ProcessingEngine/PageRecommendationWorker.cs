using DeanOBrien.XConnect.ML;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Engine.Projection;
using Sitecore.Processing.Engine.Storage.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeanOBrien.XConnect.ProcessingEngine
{
    public class PageRecommendationWorker : IDeferredWorker
    {
        public const string OptionSourceTableName = "sourceTableName";

        public const string OptionTargetTableName = "targetTableName";

        public const string OptionSchemaName = "schemaName";

        public const string OptionLimit = "limit";

        private readonly ITableStore _tableStore = null;

        private readonly string _sourceTableName = null;

        private readonly string _targetTableName = null;

        private readonly int _limit = 1;
        private readonly IPageRecommender _machineLearning;
        private IEnumerable<string> _pageIds;
        private ILogger _logger;

        public PageRecommendationWorker(
            IPageRecommender machineLearning,
            ITableStoreFactory tableStoreFactory,
            IReadOnlyDictionary<string, string> options,
            ILogger logger)
        {
            _sourceTableName = options[OptionSourceTableName];
            _targetTableName = options[OptionTargetTableName];
            _limit = int.Parse(options[OptionLimit]);
            _machineLearning = machineLearning;

            var schemaName = options[OptionSchemaName];
            _tableStore = tableStoreFactory.Create(schemaName);
            _pageIds = GetPages();
            _logger = logger;


        }

        private static List<string> GetPages()
        {
            var pages = new[]
            {
                new Guid("{1CC149B3-D802-43B3-9788-170A34105D64}"),
                new Guid("{7790B721-E636-4B23-A1E1-8AC37AB0E718}"),
                new Guid("{6A3D9D1F-2B27-41A2-84ED-0339FF216405}"),
                new Guid("{32592E38-BDD1-4B8A-86B9-D62091D798F2}"),
                new Guid("{0355BFFB-64D1-4326-8BC4-EE45F54F6D8F}"),
                new Guid("{048B0F02-D6F0-45EA-8406-DDC2C335EEDD}"),
                new Guid("{A4E1B247-48AF-4E5D-B96A-0D507E8597F5}"),
                new Guid("{A96B8038-2BA3-4132-B716-B3945F57F0DD}"),
                new Guid("{7A785FDE-FED9-4CEC-B7C6-4F9BB0556F8D}"),
                new Guid("{7A785FDE-FED9-4CEC-B7C6-4F9BB0556F8D}")
            };


            return pages.ToList().Select(x => x.ToString()).ToList();
        }
        public void Dispose()
        {
            _tableStore.Dispose();
        }

        public async Task RunAsync(CancellationToken token)
        {

            var sourceRows = await _tableStore.GetRowsAsync(_sourceTableName, CancellationToken.None);

            var targetRows = new List<DataRow>();
            var targetSchema = new RowSchema(
                new FieldDefinition("ContactId", FieldKind.Key, FieldDataType.Guid),
                new FieldDefinition("PageIds", FieldKind.Key, FieldDataType.String),
                new FieldDefinition("Score", FieldKind.Key, FieldDataType.String)
            );

            while (await sourceRows.MoveNext())
            {
                foreach (var row in sourceRows.Current)
                {
                    var results = new List<Result>();

                    foreach (var id in _pageIds)
                    {
                        var result = new Result() { pageId = id };
                        result.score = _machineLearning.Predict(row["ContactId"].ToString(), id);
                        results.Add(result);
                    }

                    foreach (var page in results.OrderByDescending(x => x.score).Take(5))
                    {
                        var targetRow = new DataRow(targetSchema);
                        targetRow.SetGuid(0, new Guid(row["ContactId"].ToString()));
                        targetRow.SetString(1, page.pageId);
                        targetRow.SetString(2, page.score.ToString());
                        targetRows.Add(targetRow);
                    }

                }
            }

            // Populate the rows into the target table.
            var tableDefinition = new TableDefinition(_targetTableName, targetSchema);
            var targetTable = new InMemoryTableData(tableDefinition, targetRows);
            await _tableStore.PutTableAsync(targetTable, TimeSpan.FromMinutes(30), CancellationToken.None);
        }
    }
    public class Result
    {
        public string pageId { get; set; }
        public float score { get; set; }
    }
}
