using _301153142_301137955_Soto_Ko_Lab3.Models.Review;
using System.Diagnostics.Eventing.Reader;

namespace _301153142_301137955_Soto_Ko_Lab3.Models.Movie
{
    public class DetailsViewModel
    {
        public MovieModel? Movie { get; set; }

        public ReviewViewModel ReviewViewModel { get; set; } = new();

        public string Comment { get; set; } = string.Empty;
        public double Rating { get; set; } = 0;
        public bool IsAddBtnHidden { get; set; } = false;
    }
}
