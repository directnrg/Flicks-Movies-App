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
            QueryFilter filter = new();
            filter.AddCondition(Constants.TYPE, QueryOperator.Equal, Constants.CAP_MOVIE);
            var query = context.FromQueryAsync<MovieModel>(new QueryOperationConfig{IndexName=Constants.GSI_TYPE_TIMESTAMP, BackwardSearch=true, Filter= filter});
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
                    IndexName = Constants.GSI_USER_MOVIE,
                    QueryFilter = new List<ScanCondition>
                    {
                        new ScanCondition(Constants.TYPE, ScanOperator.Equal, Constants.CAP_MOVIE)
                    }
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

        /* search */
        internal static async Task<List<MovieModel>> GetMoviesByGenre(string genre)
        {
            ScanFilter scanFilter = new();
            scanFilter.AddCondition(Constants.GENRE, ScanOperator.Contains, genre);
            var query = context.FromScanAsync<MovieModel>(new ScanOperationConfig { IndexName = Constants.GSI_GENRE, Filter = scanFilter });
            List<MovieModel> movies = await query.GetRemainingAsync();
            return movies;
        }

        public static async Task<List<MovieModel>> GetMoviesByAvgRating(double min, double max)
        {
            ScanFilter scanFilter = new();
            scanFilter.AddCondition(Constants.AVG_RATING, ScanOperator.Between, min, max);
            var query = context.FromScanAsync<MovieModel>(new ScanOperationConfig { IndexName = Constants.GSI_AVG_RATING, Filter = scanFilter });
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

        internal static async Task UpdateMovie(MovieModel updatedMovie)
        {
            try
            {
                //passing movie id without prefix
                var dbMovie = await GetMovieById(updatedMovie.MovieId.Substring(6));

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
                if (updatedMovie.Genre != null)
                {
                    dbMovie.Genre = updatedMovie.Genre;
                }
                if (updatedMovie.ReleasedDate != null)
                {
                    dbMovie.ReleasedDate = updatedMovie.ReleasedDate;
                }

                //Saving overwrites the data of the whole object so updated dbMovie object is returned here to make sure other properties are not deleted.
                await context.SaveAsync(dbMovie);
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating movie: {ex.Message}");
                throw;
            }

        }

        internal static async Task<string> DeleteMovie(MovieModel movieToDelete)
        {
            try
            {
                await context.DeleteAsync(movieToDelete);
                return Constants.SUCCESS;
            }
            catch (Exception ex)
            {
                return $"{Constants.ERROR} while deleting movie data: {ex.Message}";
            }
        }

        /* methods to be implemented */
        public static async Task<List<CommentModel>> GetCommentsInLast24h(string movieId)
        {
            var oneDayAgo = DateTime.UtcNow.AddHours(-24).ToString("s");

            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition(Constants.MOVIE_ID, ScanOperator.Equal, $"{Constants.CAP_COMMENT}{movieId}"),
                new ScanCondition(Constants.TIMESTAMP, ScanOperator.Between, oneDayAgo, DateTime.UtcNow.ToString("o"))
            };
            // should specify the gsi used
            return await context.ScanAsync<CommentModel>(scanConditions).GetRemainingAsync();
        }
    }
}
