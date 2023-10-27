using _301153142_301137955_Soto_Ko_Lab3.Models;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace _301153142_301137955_Soto_Ko_Lab3.AWS
{
    public static class DynamoDBService
    {
        readonly static DynamoDBContext context;

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

        internal static async Task<List<MovieModel>> GetMoviesByUserId(string userId)
        {
            Debug.WriteLine($"user ID: {userId}");

            try
            {
                var config = new DynamoDBOperationConfig
                {
                    IndexName = Constants.GSI_USER_MOVIE
                };

                // Use the userId as the hash key value for the GSI and pass the config for querying
                var moviesQuery = context.QueryAsync<MovieModel>(userId, config);
                List<MovieModel> movies = await moviesQuery.GetNextSetAsync();

                return movies;
            }
            catch
            {
                throw;
            }
        }



        internal static async Task<List<MovieModel>> GetMoviesByGenre(string genre)
        {
            ScanFilter scanFilter = new();
            scanFilter.AddCondition(Constants.GENRE, ScanOperator.Contains, genre);
            var query = context.FromScanAsync<MovieModel>(new ScanOperationConfig { IndexName = Constants.GSI_GENRE, Filter = scanFilter });
            List<MovieModel> movies = await query.GetRemainingAsync();
            return movies;
        }

        internal static async Task<string> AddMovieItem(MovieModel movie)
        {
            try
            {
                await context.SaveAsync(movie);
                return Constants.SUCCESS;
            }
            catch (Exception ex)
            {
                return $"{Constants.ERROR} while adding movie data: {ex.Message}";
            }
        }

        internal static async Task<MovieModel> GetMovieById(string movieId)
        {
            try
            {
                QueryFilter filter = new(Constants.MOVIE_ID, QueryOperator.Equal, Constants.CAP_MOVIE + movieId);
                var query = context.FromQueryAsync<MovieModel>(new QueryOperationConfig { Filter = filter });
                List<MovieModel> movie = await query.GetRemainingAsync();
                return movie.ElementAt(0);
            }
            catch
            {
                throw;
            }
        }

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


        /* methods to be implemented */

        public static async Task<List<CommentModel>> GetCommentsInLast24h(string movieId)
        {
            var oneDayAgo = DateTime.UtcNow.AddHours(-24).ToString("o");

            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition(Constants.MOVIE_ID, ScanOperator.Equal, $"{Constants.CAP_COMMENT}{movieId}"),
                new ScanCondition(Constants.COMMENT_TIMESTAMP, ScanOperator.Between, oneDayAgo, DateTime.UtcNow.ToString("o"))
            };
            // should specify the gsi used
            return await context.ScanAsync<CommentModel>(scanConditions).GetRemainingAsync();
        }


        internal static async Task UpdateMovie(MovieModel updatedMovie)
        {
            try
            {
                Debug.WriteLine($"Updated Movie object properties:\n" +
                $"MovieId: {updatedMovie.MovieId}\n" +
                $"UserId: {updatedMovie.UserId}\n" +
                $"Title: {updatedMovie.Title}\n" +
                $"Directors: {string.Join(", ", updatedMovie.Directors)}\n" +
                $"Genre: {updatedMovie.Genre}\n" +
                $"ReleasedDate: {updatedMovie.ReleasedDate}\n");

                //passing movie id without prefix
                var dbMovie = await GetMovieById(updatedMovie.MovieId.Substring(6));

                Debug.WriteLine($"current Movie object properties:\n" +
                $"MovieId: {dbMovie.MovieId}\n" +
                $"UserId: {dbMovie.UserId}\n" +
                $"Title: {dbMovie.Title}\n" +
                $"Directors: {string.Join(", ", dbMovie.Directors)}\n" +
                $"Genre: {dbMovie.Genre}\n" +
                $"ReleasedDate: {dbMovie.ReleasedDate}\n");

                if (dbMovie == null)
                {
                    throw new Exception("Movie not found");
                }

                // safety check to update only properties provided if they are not empty.
                if (updatedMovie.Title != null)
                {
                    dbMovie.Title = updatedMovie.Title;
                }
                if (updatedMovie.Directors != null)
                {
                    dbMovie.Directors = updatedMovie.Directors;
                }
                if (updatedMovie.Genre != null) {
                    dbMovie.Genre = updatedMovie.Genre;
                }
                if (updatedMovie.ReleasedDate != null)
                {
                    dbMovie.ReleasedDate = updatedMovie.ReleasedDate;
                }

                await context.SaveAsync(dbMovie);

            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating movie: {ex.Message}");
                throw;
            }

        }
    }
}
