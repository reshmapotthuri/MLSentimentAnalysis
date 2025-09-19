using Microsoft.ML;
using SampleMLBot.Models;
using System;

namespace SampleMLBot.Bots
{
    public static class ModelTrainer 
    {
        private static readonly string datapath = @"D:\Reshma-Projects\SampleMLBot\TrainingData.csv";
        private static readonly string modelpath = "chatbotModel.zip";
        public static void TrainModel()
        {
            var mlContext = new MLContext();

            //Load Data
            IDataView dataView = mlContext.Data.LoadFromTextFile<ChatData>(datapath, separatorChar: ',', hasHeader: true);

            //Define pipeline
            //var pipeLine = mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(ChatData.Analysis))
            //               .Append(mlContext.Transforms.Text.FeaturizeText("Features", nameof(ChatData.Message)))
            //               .Append(mlContext.MulticlassClassification.Trainers.OneVersusAll(binaryEstimator: mlContext.BinaryClassification.Trainers.FastForest()))
            //               .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
            var pipeLine=SampleMLBot.MLSentimentAnalysis.BuildPipeline(mlContext);
            //Train Model
            var model = pipeLine.Fit(dataView);

            //save Model
            mlContext.Model.Save(model, dataView.Schema, modelpath);
            Console.WriteLine("Model trained and saved succesfuly");


        }
    }
}
