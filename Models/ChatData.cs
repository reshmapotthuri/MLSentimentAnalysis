using Microsoft.ML.Data;

namespace SampleMLBot.Models
{
    public class ChatData
    {
        [LoadColumn(0)]
        public string Message { get; set; }
        [LoadColumn(1)]
        public string Analysis { get; set; }
  
    }

    public class ChatPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Analysis { get; set; }  
        [ColumnName("Score")]
        public float[] Score { get; set; }  
    }
    public enum SentimentLabels
    {
        Positive = 0,
        Neutral = 1,
        Negative =2

    }
}
