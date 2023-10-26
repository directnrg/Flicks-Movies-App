using Amazon.DynamoDBv2.DataModel;

namespace _301153142_301137955_Soto_Ko_Lab3.Models
{
    public class CommentModel
    {
        [DynamoDBHashKey]
        public string MovieId { get; set; }
        [DynamoDBRangeKey]
        public string UserId { get; set; }

        public string Comment { get; set; }
        public string CommentTimestamp { get; set; }

        // comment
        public CommentModel(string movieIdwithNoPrefix, string userId, string comment)
        {
            MovieId = Constants.CAP_COMMENT + movieIdwithNoPrefix;
            UserId = userId;
            Comment = comment;
            CommentTimestamp = DateTime.UtcNow.ToString();
        }
    }
}
