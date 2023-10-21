using Amazon.DynamoDBv2.DataModel;

namespace _301153142_301137955_Soto_Ko_Lab3.Models
{
    [DynamoDBTable("Movie")]
    public class ReviewModel
    {
        [DynamoDBHashKey]
        public string MovieId { get; set; }

        [DynamoDBRangeKey]
        public string UserId { get; set; }
        public double Rating { get; set; }
        public string RatingTimestamp { get; set; }
        public string Comment { get; set; }
        public string CommentTimestamp { get; set; }
        
        // rating + comment
        public ReviewModel(string movieIdwithNoPrefix, string userId, double rating, string comment)
        {
            MovieId = Constants.CAP_REVIEW + movieIdwithNoPrefix;
            UserId = userId;
            Rating = rating;
            Comment = comment;
            RatingTimestamp = DateTime.UtcNow.ToString();
            CommentTimestamp = DateTime.UtcNow.ToString();
        }

        // rating
        public ReviewModel(string movieIdwithNoPrefix, string userId, double rating)
        {
            MovieId = Constants.CAP_REVIEW + movieIdwithNoPrefix;
            UserId = userId;
            Rating = rating;
            RatingTimestamp = DateTime.UtcNow.ToString();
        }

        // comment
        public ReviewModel(string movieIdwithNoPrefix, string userId, string comment)
        {
            MovieId = Constants.CAP_REVIEW + movieIdwithNoPrefix;
            UserId = userId;
            Comment = comment;
            CommentTimestamp = DateTime.UtcNow.ToString();
        }

    }
}
