using Amazon.DynamoDBv2.DataModel;

namespace _301153142_301137955_Soto_Ko_Lab3.Models
{
    [DynamoDBTable("Movie")]
    public class MovieModel
    {
        [DynamoDBHashKey]
        public string MovieId { get; set; }
        [DynamoDBRangeKey]
        public string UserId { get; set; }

        public string Title { get; set; }
        public List<string> Directors { get; set; }
        public string Genre { get; set; }
        public double AvgRating { get; set; }
        public int NumOfRatings { get; set; }
        public string ReleasedDate { get; set; }
        public string ThumbnailS3Key { get; set; }
        public string VideoS3Key { get; set; }
        public string Timestamp { get; set; }
        public string Type { get; set; }

        [DynamoDBIgnore]
        public IFormFile? Thumbnail { get; set; }
        [DynamoDBIgnore]
        public IFormFile? Video { get; set; }

        public MovieModel() {}
        public MovieModel(string movieIdwithNoPrefix, string userId, string title)
        {
            Type = Constants.PREFIX_MOVIE;
            MovieId = Constants.PREFIX_MOVIE + movieIdwithNoPrefix;
            Title = title;
            UserId = userId;
            NumOfRatings = 0;
            AvgRating = 0;
            Timestamp = DateTime.UtcNow.ToString("s");
        }
    }
}
