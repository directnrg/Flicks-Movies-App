namespace _301153142_301137955_Soto_Ko_Lab3
{
    public static class Constants
    {
        public static string PREFIX_MOVIE = "MOVIE#";
        public static string PREFIX_COMMENT = "COMMENT#";
        public static string PREFIX_RATING = "RATING#";
        public static string MOVIE_ID = "MovieId";
        public static string USER_ID = "UserId";
        public static string TIMESTAMP = "Timestamp";
        public static string TYPE = "Type";
        public static string GENRE = "Genre";
        public static string AVG_RATING = "AvgRating";
        public static string TITLE = "Title";
        public static string VIDEO = "Video";
        public static string IMAGE = "image";

        public static char FORWARD_SLASH = '/';
        public static char COMMA = ',';
        public static string COMMA_DELIMITER = ", ";
        public static string PERIOD = ".";
        public static string HASHTAG = "#";

        // msg related
        public static string SUCCESS = "SUCCESS";
        public static string ERROR = "ERROR";

        // bucket name
        public static string MOVIE_BUCKET_NAME = "comp306-lab3-movies-sk";
        public static string VIDEO_FOLDER = "movies/";
        public static string THUMBNAIL_FOLDER = "thumbnails/";

        // GSI
        public static string GSI_GENRE = "Genre-index";
        public static string GSI_AVG_RATING = "AvgRating-index";
        public static string GSI_TYPE_TIMESTAMP = "Type-Timestamp-index";
        public static string GSI_USER_MOVIE = "UserId-MovieId-index";

        //VIEWS 
        public static string VIEW_DETAILS = "Details";
        public static string VIEW_ERROR = "Error";
        public static string VIEW_UPDATE = "Update";

        //TempData
        public static string TEMP_MOVIE = "Movie";
        public static string TEMP_COMMENTS = "Comments";
        public static string TEMP_RATINGS = "Ratings";


        // action
        public static string UPDATE = "update";
        public static string DELETE = "delete";
    }
}
