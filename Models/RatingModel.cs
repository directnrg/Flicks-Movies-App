using Amazon.DynamoDBv2.DataModel;

namespace Flicks_App.Models
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
