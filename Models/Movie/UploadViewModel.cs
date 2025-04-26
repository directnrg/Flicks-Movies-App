using Flicks_App.Enums;

namespace Flicks_App.Models.Movie
{
    public class UploadViewModel
    {
        public MovieModel Movie { get; set; }
        public List<MovieGenre> SelectedGenres { get; set; }

        public string ConvertSelectedGenresToString()
        {
            if (SelectedGenres == null || !SelectedGenres.Any())
                return string.Empty;

            IEnumerable<string> readableGenres = SelectedGenres.Select(genre => MovieGenreExtension.EnumToReadableString(genre));
            return string.Join(Constants.COMMA_DELIMITER, readableGenres);
        }
    }
}
