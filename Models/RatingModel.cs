using Amazon.DynamoDBv2.DataModel;

namespace _301153142_301137955_Soto_Ko_Lab3.Models
{
    [DynamoDBTable("Movie")]
    public class RatingModel
    {
        [DynamoDBHashKey]
        public string MovieId { get; set; }

        [DynamoDBRangeKey]
        public string UserId { get; set; }
        public double Rating { get; set; }
        public string Timestamp { get; set; }
        public string Type { get; set; }

        // rating
        public RatingModel(string movieIdwithNoPrefix, string userId, double rating)
        {
            Type = Constants.PREFIX_RATING;
            MovieId = Constants.PREFIX_RATING + movieIdwithNoPrefix;
            UserId = userId;
            Rating = rating;
            Timestamp = DateTime.UtcNow.ToString("s");
        }
        public RatingModel() { }
    }
}
