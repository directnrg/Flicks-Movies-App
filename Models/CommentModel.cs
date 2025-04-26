using Amazon.DynamoDBv2.DataModel;

namespace Flicks_App.Models
{
    [DynamoDBTable("Movie")]
    public class CommentModel
    {
        [DynamoDBHashKey]
        public string MovieId { get; set; }
        [DynamoDBRangeKey]
        public string UserId { get; set; }

        public string Comment { get; set; }
        public string Timestamp { get; set; }
        public string Type { get; set; }

        public CommentModel(string movieIdwithNoPrefix, string userId, string comment)
        {
            Type = Constants.PREFIX_COMMENT;
            MovieId = Constants.PREFIX_COMMENT + movieIdwithNoPrefix;
            UserId = userId;
            Comment = comment;
            Timestamp = DateTime.UtcNow.ToString("s");
        }
        public CommentModel() { }

    }
}
