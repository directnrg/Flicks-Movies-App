using _301153142_301137955_Soto_Ko_Lab3.Models;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace _301153142_301137955_Soto_Ko_Lab3.AWS
{
    public static class DynamoDBService
    {
        static DynamoDBContext context;
        /*
         * get the list of movie meta data items
         */
        public static async Task<List<MovieModel>> GetAllMovies()
        {
            var request = new QueryRequest
            {
                TableName = "YourTableName",
                FilterExpression = "attribute_exists(title) OR size(title) > 0",
            };

            QueryResponse res = await AWSClients.dynamoClient.QueryAsync(request);

            List<MovieModel> movies = new ();

            foreach (var m in res.Items)
            {
                MovieModel movie = CreateMovieFromItem(m);

                movies.Add(movie);
            }
            return movies;
        }


        private static MovieModel CreateMovieFromItem(Dictionary<string, AttributeValue> movieItem)
        {
            return new MovieModel()
            {
                MovieId = movieItem["MovieId"].S,
                UserId = movieItem["UserId"].S,
                Title = movieItem["Title"].S,
                Genre = movieItem["Genre"].S,
                Directors = movieItem["Directors"].SS,
                AvgRating =  double.Parse(movieItem["AvgRatings"].N),
                NumOfRatings = int.Parse(movieItem["NumOfRatings"].N),
                VideoS3Key = movieItem["VideoS3Key"].S,
                ThumbnailS3Key = movieItem["ThumbnailS3Key"].S,
            };
            
        }

    }
}
