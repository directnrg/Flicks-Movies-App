namespace _301153142_301137955_Soto_Ko_Lab3.Enums
{
    public static class MovieGenreExtension
    {
        public static string EnumToReadableString(MovieGenre genre)
        {
            return genre switch
            {
                MovieGenre.Action => "Action",
                MovieGenre.Adventure => "Adventure",
                MovieGenre.Animation => "Animation",
                MovieGenre.Biography => "Biography",
                MovieGenre.Comedy => "Comedy",
                MovieGenre.Crime => "Crime",
                MovieGenre.Documentary => "Documentary",
                MovieGenre.Drama => "Drama",
                MovieGenre.Family => "Family",
                MovieGenre.Fantasy => "Fantasy",
                MovieGenre.FilmNoir => "Film Noir",
                MovieGenre.History => "History",
                MovieGenre.Horror => "Horror",
                MovieGenre.Musical => "Musical",
                MovieGenre.Mystery => "Mystery",
                MovieGenre.Romance => "Romance",
                MovieGenre.SciFi => "Science Fiction",
                MovieGenre.Sport => "Sport",
                MovieGenre.Thriller => "Thriller",
                MovieGenre.War => "War",
                MovieGenre.Suspense => "Suspense",
                MovieGenre.Western => "Western",
                MovieGenre.Superhero => "Superhero",
                MovieGenre.MartialArts => "Martial Arts",
                MovieGenre.Slasher => "Slasher",
                MovieGenre.Paranormal => "Paranormal",
                MovieGenre.Mockumentary => "Mockumentary",
                MovieGenre.Zombie => "Zombie",
                MovieGenre.Dystopian => "Dystopian",
                _ => string.Empty,
            };
        }
    }
}
