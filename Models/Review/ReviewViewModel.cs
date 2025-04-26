using Amazon.S3.Model;

namespace Flicks_App.Models.Review
{
    public class ReviewViewModel
    {
        public List<CommentModel>? Comments { get; set; } = new();
        public List<RatingModel>? Ratings { get; set; } = new();
        public List<string>? NamesOfUsers { get; set; } = new();

        public List<bool>? IsEditBtnHidden{ get; set;} = new();

    }
}
