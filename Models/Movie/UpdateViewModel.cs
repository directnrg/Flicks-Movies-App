
using _301153142_301137955_Soto_Ko_Lab3.Enums;

namespace _301153142_301137955_Soto_Ko_Lab3.Models.Movie
{
    public class UpdateViewModel
    {
        public MovieModel Movie { get; set; } = new();
        public List<MovieGenre>? SelectedGenres { get; set; }

        public string ConvertSelectedGenresToString()
        {
            if (SelectedGenres == null || !SelectedGenres.Any())
                return string.Empty;

            IEnumerable<string> readableGenres = SelectedGenres.Select(genre => MovieGenreExtension.EnumToReadableString(genre));
            return string.Join(", ", readableGenres);
        }
    }
}
