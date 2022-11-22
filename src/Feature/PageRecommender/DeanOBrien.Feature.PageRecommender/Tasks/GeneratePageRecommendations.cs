using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.XConnect.Client.Configuration;
using Sitecore.XConnect;
using Sitecore.Processing.Tasks.Options.DataSources.Search;
using Sitecore.Processing.Tasks.Options.Workers.ML;
using DeanOBrien.XConnect.Models.ProcessingEngine;
using Sitecore.Diagnostics;
using DeanOBrien.XConnect.ProcessingEngine;
using Sitecore.Data.Items;
using Sitecore.XConnect.Client;

namespace DeanOBrien.Feature.PageRecommender.Tasks
{
    public class GeneratePageRecommendations
    {
        private const int TimeoutIntervalMinutes = 90;
        private readonly List<Guid> _taskIds = null;

        private readonly TimeSpan _taskTimeout;
        private readonly TimeSpan _storageTimeout;
        private int _predictionDataSourceRangeInHrs;
        private int _trainDataSourceRangeInHrs;
        private static ITaskManager _taskManager;
        private static XConnectClient _xConnectClient;

        // Goal Definition Ids
        private static readonly Guid Tab0 = new Guid("38470D84-BA4E-4ABC-8740-9BCD66EF6E0F");
        private static readonly Guid Tab1 = new Guid("AB7FCA23-FB0F-45D3-AB3C-7C025D2CF01F");
        private static readonly Guid Tab2 = new Guid("0F049C02-0ED7-452C-905F-C15EF2B92680");
        private static readonly Guid Tab3 = new Guid("518C5ED4-B27A-4110-B1FB-C3646E9BF8A7");
        private static readonly Guid Tab4 = new Guid("F7DE34FA-251A-4ED7-B977-7010B634884A");
        private static readonly Guid Tab5 = new Guid("0E52F41D-771F-4A3D-B1FF-60DA7B87511E");
        private static readonly Guid Tab6 = new Guid("D1C9CEC3-50AE-4076-9434-B7AE9FDDDD16");
        private static readonly List<Guid> _eventsList = new List<Guid>() { Tab0,Tab1,Tab3,Tab4,Tab5,Tab6 };

        public GeneratePageRecommendations(int taskTimeout = 12, int storageTimeout = 12, int predictionDataSourceRangeInHrs = 24, int trainDataSourceRangeInHrs = 4380)
        {
            _taskIds = new List<Guid>();
            _taskTimeout = TimeSpan.FromHours(taskTimeout);
            _storageTimeout = TimeSpan.FromHours(storageTimeout);
            _predictionDataSourceRangeInHrs = predictionDataSourceRangeInHrs;
            _trainDataSourceRangeInHrs = trainDataSourceRangeInHrs;
            _taskManager = ServiceLocator.ServiceProvider.GetService<ITaskManager>();// GetTaskManager();
            _xConnectClient = SitecoreXConnectClientConfiguration.GetClient();
        }

        public async Task Run(Item[] items, string taskName)
        {
            try
            {
                // Train Model tasks
                Guid projectionTrainTaskId = await CreateProjectionTaskToTrainModel();
                Guid mergeTrainTaskId = await CreateMergeTaskToTrainModel(projectionTrainTaskId);
                Guid trainTaskId = await CreateTrainModelTask(mergeTrainTaskId);

                // Prediction tasks
                Guid projectionTaskId = await CreateProjectionTaskToMakePredictions(trainTaskId);
                Guid mergeTaskId = await CreateMergeTaskToMakePredictions(projectionTaskId);
                Guid predictionsTaskId = await CreateMakePredictionsTask(mergeTaskId);
                await CreateStorePredictionsTask(predictionsTaskId);

            }
            catch (Exception ex)
            {

                Log.Info($"PROBLEM  Generate Page Recommendations: Failed \r\n {ex.ToString()}", this);
            }
        }
        #region Training Tasks
        private async Task<Guid> CreateProjectionTaskToTrainModel()
        {
            var query = _xConnectClient.Interactions.Where(interaction =>
                                    interaction.Events.Any(ev => _eventsList.Contains(ev.DefinitionId))
                                    && interaction.StartDateTime > DateTime.Now.AddHours(-_trainDataSourceRangeInHrs))
                                    .WithExpandOptions(new InteractionExpandOptions { Contact = new RelatedContactExpandOptions() });

            var searchRequest = query.GetSearchRequest();

            var dataSourceOptions = new InteractionSearchDataSourceOptionsDictionary(
                searchRequest, // searchRequest
                30, // maxBatchSize
                50 // defaultSplitItemCount
            );

            var projectionOptions = new InteractionProjectionWorkerOptionsDictionary(
                typeof(PageRecommendationModel).AssemblyQualifiedName, // modelTypeString
                _storageTimeout, // timeToLive
                "train-recommendation", // schemaName
                new Dictionary<string, string> // modelOptions
                {
                    { PageRecommendationModel.OptionTableName, "contactPages" }
                }
            );

            var projectionTaskId = await _taskManager.RegisterDistributedTaskAsync(
                dataSourceOptions, // datasourceOptions
                projectionOptions, // workerOptions
                null, // prerequisiteTaskIds
                _taskTimeout // expiresAfter
            );

            Log.Info($"Registered projection task {projectionTaskId}", "CreateProjectionTaskToTrainModel");
            return projectionTaskId;
        }
        private async Task<Guid> CreateMergeTaskToTrainModel(Guid projectionTaskId)
        {
            var mergeOptions = new MergeWorkerOptionsDictionary(
                "contactPagesFinal", // tableName
                "contactPages", // prefix
                _storageTimeout, // timeToLive
                "train-recommendation" // schemaName
            );

            var mergeTaskId = await _taskManager.RegisterDeferredTaskAsync(
                mergeOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    projectionTaskId
                },
                _taskTimeout // expiresAfter
            );

            Log.Info($"Registered merge task {mergeTaskId}", "CreateMergeTaskToTrainModel");
            return mergeTaskId;
        }
        private async Task<Guid> CreateTrainModelTask(Guid mergeTaskId)
        {
            var trainingOptions = new DeferredWorkerOptionsDictionary(
                typeof(PageTrainingDeferredWorker).AssemblyQualifiedName, // workerType
                new Dictionary<string, string> // options
                {
                    { PageTrainingDeferredWorker.OptionSourceTableName, "contactPagesFinal" },
                    { PageTrainingDeferredWorker.OptionTargetTableName, string.Empty },
                    { PageTrainingDeferredWorker.OptionSchemaName, "train-recommendation" },
                    { PageTrainingDeferredWorker.OptionLimit, "5" }
                }
            );

            var trainingTaskId = await _taskManager.RegisterDeferredTaskAsync(
                trainingOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    mergeTaskId
                },
                _taskTimeout // expiresAfter
            );

            Log.Info($"Registered training task {trainingTaskId}", "CreateTrainModelTask");
            return trainingTaskId;
        }
        #endregion
        #region Prediction Tasks

        private async Task<Guid> CreateProjectionTaskToMakePredictions(Guid prerequisiteTaskId)
        {
            var query = _xConnectClient.Interactions.Where(interaction =>
                                interaction.Events.Any(ev => _eventsList.Contains(ev.DefinitionId))
                                && interaction.StartDateTime > DateTime.Now.AddHours(-_predictionDataSourceRangeInHrs))
                                .WithExpandOptions(new InteractionExpandOptions { Contact = new RelatedContactExpandOptions() });

            var searchRequest = query.GetSearchRequest();

            var dataSourceOptions = new InteractionSearchDataSourceOptionsDictionary(
                searchRequest, // searchRequest
                30, // maxBatchSize
                50 // defaultSplitItemCount
            );
            var projectionOptions = new InteractionProjectionWorkerOptionsDictionary(
                typeof(ContactModel).AssemblyQualifiedName, // modelTypeString
                _storageTimeout, // timeToLive
                "generate-recommendation", // schemaName
                new Dictionary<string, string> // modelOptions
                {
                    { ContactModel.OptionTableName, "contact" }
                }
            );
            var projectionTaskId = await _taskManager.RegisterDistributedTaskAsync(
                dataSourceOptions, // datasourceOptions
                projectionOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    prerequisiteTaskId
                },
                _taskTimeout // expiresAfter
            );
            Log.Info($"Registered ContactModel projection task {projectionTaskId}", this);
            return projectionTaskId;
        }

        private async Task<Guid> CreateMergeTaskToMakePredictions(Guid projectionTaskId)
        {
            var mergeOptions = new MergeWorkerOptionsDictionary(
                "contactFinal", // tableName
                "contact", // prefix
                _storageTimeout, // timeToLive
                "generate-recommendation" // schemaName
            );

            var mergeTaskId = await _taskManager.RegisterDeferredTaskAsync(
                mergeOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    projectionTaskId
                },
                _taskTimeout // expiresAfter
            );

            Log.Info($"Registered ContactModel merge task {mergeTaskId}", this);
            return mergeTaskId;
        }
        private async Task<Guid> CreateMakePredictionsTask(Guid mergeTaskId)
        {
            var recommendationOptions = new DeferredWorkerOptionsDictionary(
                typeof(PageRecommendationWorker).AssemblyQualifiedName, // workerType
                new Dictionary<string, string> // options
                {
                    { PageRecommendationWorker.OptionSourceTableName, "contactFinal" },
                    { PageRecommendationWorker.OptionTargetTableName, "contactRecommendations" },
                    { PageRecommendationWorker.OptionSchemaName, "generate-recommendation" },
                    { PageRecommendationWorker.OptionLimit, "5" }
                }
            );

            var recommendationTaskId = await _taskManager.RegisterDeferredTaskAsync(
                recommendationOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    mergeTaskId
                },
                _taskTimeout // expiresAfter
            );

            Log.Info($"Registered ContactModel recommendation task {recommendationTaskId}", this);
            return recommendationTaskId;
        }
        private async Task CreateStorePredictionsTask(Guid recommendationTaskId)
        {
            var storeFacetOptions = new DeferredWorkerOptionsDictionary(
                                typeof(RecommendationFacetStorageWorker).AssemblyQualifiedName, // workerType
                                new Dictionary<string, string> // options
                                {
                                    { RecommendationFacetStorageWorker.OptionTableName, "contactRecommendations" },
                                    { RecommendationFacetStorageWorker.OptionSchemaName, "generate-recommendation" }
                                }
                            );

            var storeFacetTaskId = await _taskManager.RegisterDeferredTaskAsync(
                storeFacetOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    recommendationTaskId
                },
                _taskTimeout // expiresAfter
            );

            Log.Info($"Registered ContactModel store facet task {storeFacetTaskId}", this);
        }
        #endregion

    }
}
