using Amazon.S3.Model;

namespace _301153142_301137955_Soto_Ko_Lab3.Models.Review
{
    public class ReviewViewModel
    {
        public List<CommentModel>? Comments { get; set; } = new();
        public List<RatingModel>? Ratings { get; set; } = new();

    }
}
