using DeanOBrien.XConnect.Models;
using DeanOBrien.XConnect.Models.ML;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeanOBrien.XConnect.ML
{
    public class PageRecommender : IPageRecommender
    {
        private string _modelPath = Path.Combine("c:/deploy/", "Data", "PageRecommenderModel.zip");
        private string _logPath = "c:/deploy/ml.txt";
        private ILogger _logger;

        public List<PageRating> _trainingData { get; set; }
        public List<PageRating> _testData { get; set; }

        public PageRecommender(ILogger logger) { _logger = logger; }
        public float Predict(string cId, string crseId)
        {
            DataViewSchema inputSchema;
            MLContext mlContext = new MLContext();
            ITransformer model = mlContext.Model.Load(_modelPath, out inputSchema);
            var predictionEngine = mlContext.Model.CreatePredictionEngine<PageRating, PageRatingPrediction>(model);
            var input = new PageRating { contactId = cId, pageId = crseId };
            var result = predictionEngine.Predict(input).Score;
            if (!float.IsNaN(result))
            {
                _logger.LogInformation($"REPORT: MachineLearning Prediction made for {cId} ({crseId}|{result})");
            }

            return result;


        }
        public void Train(List<PageEngagement> data)
        {
            _trainingData = new List<PageRating>();
            _testData = new List<PageRating>();

            var size = data.Count();
            var trainSize = (int)Math.Ceiling(size * 0.8);
            var testSize = size - trainSize;
            var dataAsArray = data.ToArray();

            for (int i = 0; i < trainSize; i++)
            {
                _trainingData.Add(new PageRating() { contactId = dataAsArray[i].ContactId, pageId = dataAsArray[i].PageId, Label = dataAsArray[i].Engagement });
            }

            for (int i = trainSize; i < size - 1; i++)
            {
                _testData.Add(new PageRating() { contactId = dataAsArray[i].ContactId, pageId = dataAsArray[i].PageId, Label = dataAsArray[i].Engagement });
            }

            MLContext mlContext = new MLContext();
            (IDataView trainingDataView, IDataView testDataView) = LoadData(mlContext);
            ITransformer model = BuildAndTrainModel(mlContext, trainingDataView);
            EvaluateModel(mlContext, testDataView, model);
            SaveModel(mlContext, trainingDataView.Schema, model);
        }
        (IDataView training, IDataView test) LoadData(MLContext mlContext)
        {
            _logger.LogInformation("Loading Data");

            var trainingData = mlContext.Data.LoadFromEnumerable(_trainingData);
            var testData = mlContext.Data.LoadFromEnumerable(_testData);
            var pipeline = mlContext.Transforms.Concatenate("Features", "contactId", "pageId");

            IDataView trainingDataView = pipeline.Fit(trainingData).Transform(trainingData);
            IDataView testDataView = pipeline.Fit(testData).Transform(testData);

            return (trainingDataView, testDataView);
        }
        ITransformer BuildAndTrainModel(MLContext mlContext, IDataView trainingDataView)
        {
            _logger.LogInformation("Training the model");

            IEstimator<ITransformer> estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "contactIdEncoded", inputColumnName: "contactId")
            .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "pageIdEncoded", inputColumnName: "pageId"));

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "contactIdEncoded",
                MatrixRowIndexColumnName = "pageIdEncoded",
                LabelColumnName = "Label",
                NumberOfIterations = 20,
                ApproximationRank = 100
            };

            var trainerEstimator = estimator.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));
            ITransformer model = trainerEstimator.Fit(trainingDataView);

            return model;
        }
        void EvaluateModel(MLContext mlContext, IDataView testDataView, ITransformer model)
        {
            _logger.LogInformation("Evaluating model");

            var prediction = model.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(prediction, labelColumnName: "Label", scoreColumnName: "Score");

            _logger.LogInformation("Root Mean Squared Error : " + metrics.RootMeanSquaredError.ToString());
            _logger.LogInformation("RSquared: " + metrics.RSquared.ToString());
        }
        void SaveModel(MLContext mlContext, DataViewSchema trainingDataViewSchema, ITransformer model)
        {
            _logger.LogInformation($"Saving model to path {_modelPath}");
            mlContext.Model.Save(model, trainingDataViewSchema, _modelPath);
        }

        void Log(string message)
        {
            File.AppendAllText(_logPath, "\n\r");
            File.AppendAllText(_logPath, $"{message}");
        }
    }
}
