// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.22.0

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.ML;
using Microsoft.ML.Data;
using Newtonsoft.Json;
using SampleMLBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SampleMLBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        private static readonly string modelPath = "chatbotModel.zip";
        private PredictionEngine<ChatData, ChatPrediction> predictionEngine;
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            ModelTrainer.TrainModel();
            var mlContext = new MLContext();
            ITransformer model = mlContext.Model.Load(modelPath, out _);
            predictionEngine = mlContext.Model.CreatePredictionEngine<ChatData, ChatPrediction>(model);
            var mlreplyText = predictionEngine.Predict(new ChatData { Message = turnContext.Activity.Text });

           var labels = Enum.GetValues<SentimentLabels>();
           string[] arraylist = new string[mlreplyText.Score.Length];

            for (int i = 0; i < mlreplyText.Score.Length; i++)
            {
                var sentimentJson = "";
                string color = GetColor(labels[i].ToString());
                string percent = GenerateVerticalBar(mlreplyText.Score[i]);
                sentimentJson = $@"      
                            {{
                              ""type"": ""Column"",
                              ""width"": ""stretch"",
                              ""items"": [
                                {{ ""type"": ""TextBlock"", ""text"": ""{labels[i]}"" }},
                                {{ ""type"": ""TextBlock"", ""text"": ""{percent:F1}"", ""color"": ""{color}"", ""wrap"": true }}
                              ]
                            }}";

                arraylist[i] = sentimentJson;
            }
            string columnsJson = string.Join(",", arraylist);

            var cardjson = $@"
                            {{
                              ""type"": ""AdaptiveCard"",
                              ""version"": ""1.3"",
                              ""body"": [
                                {{
                                  ""type"": ""TextBlock"",
                                  ""text"": ""Sentiment Analysis"",
                                  ""weight"": ""Bolder"",
                                  ""size"": ""Medium""
                                }},
                                {{
                                  ""type"": ""ColumnSet"",
                                  ""columns"": [{columnsJson}]
                                }}
                              ],
                              ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json""
                            }}";

            var attachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardjson)
            };
            var activity = MessageFactory.Attachment(new List<Attachment> { attachment });
            await turnContext.SendActivityAsync(activity, cancellationToken);

        }
        public string GenerateVerticalBar(float score, int maxBlocks = 10)
        {
            int blocks = (int)Math.Round(Math.Abs(score) * maxBlocks);
            string percent = $"{Math.Abs(score * 100):F1}%";

            var barLines = new List<string>();

            for (int i = maxBlocks; i > 0; i--)
            {
                barLines.Add(i <= blocks ? "█" : " ");
            }

            barLines.Add(percent); // Add percentage label at the bottom

            return string.Join(Environment.NewLine, barLines);
        }
        public float[] Softmax(float[] logits)
        {
            var maxLogit = logits.Max(); // for numerical stability
            var expScores = logits.Select(l => Math.Exp(l - maxLogit)).ToArray();
            var sumExp = expScores.Sum();
            return expScores.Select(e => (float)(e / sumExp)).ToArray();
        }
        public static string GetColor(string label)
        {
            switch (label)
            {
                case nameof(SentimentLabels.Positive): return "Good";
                case nameof(SentimentLabels.Neutral): return "Warning";
                case nameof(SentimentLabels.Negative): return "Attention";
                default: return "Default";
            }

        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
