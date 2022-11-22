using DeanOBrien.XConnect.Models;
using Microsoft.Extensions.Logging;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Engine.Storage.Abstractions;
using Sitecore.XConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeanOBrien.XConnect.ProcessingEngine
{
    public class RecommendationFacetStorageWorker : IDeferredWorker
    {
        public const string OptionTableName = "tableName";

        public const string OptionSchemaName = "schemaName";

        private readonly string _tableName = null;

        private readonly ITableStore _tableStore = null;

        private readonly IXdbContext _xdbContext;
        private readonly ILogger _logger;

        public RecommendationFacetStorageWorker(
            ITableStoreFactory tableStoreFactory,
            IXdbContext xdbContext,
            IReadOnlyDictionary<string, string> options,
            ILogger logger)
        {
            _tableName = options[OptionTableName];
            var schemaName = options[OptionSchemaName];
            _tableStore = tableStoreFactory.Create(schemaName);
            _xdbContext = xdbContext;
            _logger = logger;
        }

        public void Dispose()
        {
            _tableStore.Dispose();
        }

        public async Task RunAsync(CancellationToken token)
        {
            var rows = await _tableStore.GetRowsAsync(_tableName, CancellationToken.None);

            while (await rows.MoveNext())
            {
                foreach (var row in rows.Current)
                {
                    var contactId = row.GetGuid(0);
                    var pageId = row.GetString(1);
                    var score = row.GetString(2);

                    var contact = await _xdbContext.GetContactAsync(contactId,
                        new ContactExecutionOptions(new ContactExpandOptions(PageRecommendationFacet.DefaultFacetKey)));

                    var facet = contact.GetFacet<PageRecommendationFacet>(PageRecommendationFacet.DefaultFacetKey) ??
                                new PageRecommendationFacet();

                    if (facet.PageRecommendations.All(x => x.PageId != pageId))
                    {
                        if (facet.PageRecommendations.Count > 4)
                        {
                            facet.PageRecommendations.OrderBy(x => x.DateRecommended);
                            facet.PageRecommendations.RemoveAt(facet.PageRecommendations.Count - 1);
                        }

                        facet.PageRecommendations.Add(new PageRecommendation()
                        {
                            PageId = pageId,
                            Score = Double.Parse(score),
                            DateRecommended = DateTime.Now
                        });
                        _logger.LogInformation($"REPORT: RecommendationFacetStorageWorker: Added Recommendation for contact {contactId} ({pageId}|{Double.Parse(score)}|{DateTime.Now})", this);

                        _xdbContext.SetFacet(contact, PageRecommendationFacet.DefaultFacetKey, facet);
                        await _xdbContext.SubmitAsync(CancellationToken.None);
                    }
                    else if (facet.PageRecommendations.Any(x => x.PageId == pageId) && facet.PageRecommendations.Where(x => x.PageId == pageId).FirstOrDefault().Score != Double.Parse(score))
                    {
                        var toRemove = facet.PageRecommendations.Where(x => x.PageId == pageId).FirstOrDefault();
                        facet.PageRecommendations.Remove(toRemove);

                        facet.PageRecommendations.Add(new PageRecommendation()
                        {
                            PageId = pageId,
                            Score = Double.Parse(score),
                            DateRecommended = DateTime.Now
                        });
                        _logger.LogInformation($"REPORT: RecommendationFacetStorageWorker: Updated Recommendation for contact {contactId} ({pageId}|{Double.Parse(score)}|{DateTime.Now})", this);

                        _xdbContext.SetFacet(contact, PageRecommendationFacet.DefaultFacetKey, facet);
                        await _xdbContext.SubmitAsync(CancellationToken.None);
                    }
                }
            }
            await _tableStore.RemoveAsync(_tableName, CancellationToken.None);
        }
    }
}
