namespace _301153142_301137955_Soto_Ko_Lab3
{
    public static class Constants
    {
        public static string CAP_MOVIE = "MOVIE#";
        public static string CAP_REVIEW = "REVIEW#";
        public static string MOVIE_ID = "MovieId";
        public static string USER_ID = "UserId";
        public static string COMMENT_TIMESTAMP = "CommentTimestamp";
        public static string GENRE = "Genre";
        public static string AVG_RATING = "AvgRating";
        public static string TITLE = "Title";
        public static string VIDEO = "Video";
        public static string IMAGE = "image";

        public static char FORWARD_SLASH = '/';
        public static char COMMA = ',';
        public static string PERIOD = ".";

        // msg related
        public static string SUCCESS = "SUCCESS";
        public static string ERROR = "ERROR";

        // bucket name
        public static string VIDEO_BUCKET_NAME = "comp306-lab3-sk/movies/";
        public static string THUMBNAIL_BUCKET_NAME = "comp306-lab3-sk/thumbnails/";
        
        // GSI
        public static string GSI_GENRE = "MovieId-Genre-index";
        public static string GSI_AVG_RATING = "MovieId-AvgRating-index";
        public static string GSI_COMMENT_TIMESTAMP = "MovieId-CommentTimestamp-index";
    }
}
