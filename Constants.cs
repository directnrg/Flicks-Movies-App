namespace Flicks_App
{
    public static class Constants
    {
        public static readonly string PREFIX_MOVIE = "MOVIE#";
        public static readonly string PREFIX_COMMENT = "COMMENT#";
        public static readonly string PREFIX_RATING = "RATING#";
        public static readonly string MOVIE_ID = "MovieId";
        public static readonly string USER_ID = "UserId";
        public static readonly string TIMESTAMP = "Timestamp";
        public static readonly string TYPE = "Type";
        public static readonly string GENRE = "Genre";
        public static readonly string AVG_RATING = "AvgRating";
        public static readonly string TITLE = "Title";
        public static readonly string VIDEO = "Video";
        public static readonly string IMAGE = "image";

        public static readonly char FORWARD_SLASH = '/';
        public static readonly char COMMA = ',';
        public static readonly string COMMA_DELIMITER = ", ";
        public static readonly string PERIOD = ".";
        public static readonly string HASHTAG = "#";

        // msg related
        public static readonly string SUCCESS = "SUCCESS";
        public static readonly string ERROR = "ERROR";
        public static readonly string NOT_AUTHORIZED_MSG = "This action is not authorized for this user.";

        // bucket name
        public static readonly string MOVIE_BUCKET_NAME = "comp306-lab3-movies-sk";
        public static readonly string VIDEO_FOLDER = "movies/";
        public static readonly string THUMBNAIL_FOLDER = "thumbnails/";

        // GSI
        public static readonly string GSI_GENRE = "Genre-index";
        public static readonly string GSI_AVG_RATING = "AvgRating-index";
        public static readonly string GSI_TYPE_TIMESTAMP = "Type-Timestamp-index";
        public static readonly string GSI_USER_MOVIE = "UserId-MovieId-index";

        //VIEWS 
        public static readonly string VIEW_DETAILS = "Details";
        public static readonly string VIEW_ERROR = "Error";
        public static readonly string VIEW_UPDATE = "Update";

        //TempData
        public static readonly string TEMP_MOVIE = "Movie";
        public static readonly string TEMP_COMMENTS = "Comments";
        public static readonly string TEMP_RATINGS = "Ratings";

        // action
        public static readonly string UPDATE = "update";
        public static readonly string DELETE = "delete";

        // session
        public static readonly string SESSION_THUMB64 = "ThumbnailBase64";
        public static readonly string SESSION_CONTENT_TYPE = "ThumbnailContentType";
    }
}
