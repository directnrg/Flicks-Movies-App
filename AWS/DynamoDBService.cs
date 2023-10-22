using _301153142_301137955_Soto_Ko_Lab3.Models;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace _301153142_301137955_Soto_Ko_Lab3.AWS
{
    public static class DynamoDBService
    {
        static DynamoDBContext context;

        static DynamoDBService()
        {
            context = new DynamoDBContext(AWSClients.dynamoClient);
        }

        /*
         * get the list of movie meta data items
         */
        internal static async Task<List<MovieModel>> GetAllMovies()
        {
            ScanFilter scanFilter = new();
            scanFilter.AddCondition(Constants.MOVIE_ID, ScanOperator.Contains, Constants.CAP_MOVIE);
            var query = context.FromScanAsync<MovieModel>(new ScanOperationConfig{ Filter= scanFilter});
            List<MovieModel> movies = await query.GetRemainingAsync();
            return movies;
        }

        internal static async Task<List<MovieModel>> GetMoviesByGenre(string genre)
        {
            ScanFilter scanFilter = new();
            scanFilter.AddCondition(Constants.GENRE, ScanOperator.Contains, genre);
            var query = context.FromScanAsync<MovieModel>(new ScanOperationConfig { Filter = scanFilter });
            List<MovieModel> movies = await query.GetRemainingAsync();
            return movies;
        }

        /* methods to be implemented */
        public static async Task<List<MovieModel>> GetMoviesByAvgRating(double min, double max)
        {
            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition(Constants.MOVIE_ID, ScanOperator.Contains, Constants.CAP_MOVIE),
                new ScanCondition(Constants.AVG_RATING, ScanOperator.Between, min, max)
            };
            // should specify the gsi used
            return await context.ScanAsync<MovieModel>(scanConditions).GetRemainingAsync();
        }


        public static async Task<List<ReviewModel>> GetCommentsInLast24h(string movieId)
        {
            var oneDayAgo = DateTime.UtcNow.AddHours(-24).ToString("o");

            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition(Constants.MOVIE_ID, ScanOperator.Equal, $"{Constants.CAP_REVIEW}{movieId}"),
                new ScanCondition(Constants.COMMENT_TIMESTAMP, ScanOperator.Between, oneDayAgo, DateTime.UtcNow.ToString("o"))
            };
            // should specify the gsi used
            return await context.ScanAsync<ReviewModel>(scanConditions).GetRemainingAsync();
        }
    }
}
