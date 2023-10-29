using _301153142_301137955_Soto_Ko_Lab3.Models;
using Amazon.DynamoDBv2;
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
            filter.AddCondition(Constants.TYPE, ScanOperator.Equal, Constants.PREFIX_MOVIE);
            var query = context.FromQueryAsync<MovieModel>(new QueryOperationConfig{IndexName=Constants.GSI_TYPE_TIMESTAMP, BackwardSearch=true, Filter= filter});
            List<MovieModel> movies = await query.GetRemainingAsync();
            return movies;
        }

        internal static async Task<List<MovieModel>> GetMoviesByUserId(string userId)
        {
            Debug.WriteLine($"IDs at GetMoviesByUserId:\nuserId: {userId}");

            try
            {
                var config = new DynamoDBOperationConfig
                {
                    IndexName = Constants.GSI_USER_MOVIE,
                    QueryFilter = new List<ScanCondition>
                    {
                        new ScanCondition(Constants.TYPE, ScanOperator.Equal, Constants.PREFIX_MOVIE)
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
                QueryFilter filter = new(Constants.MOVIE_ID, QueryOperator.Equal, Constants.PREFIX_MOVIE + movieId);
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


        internal static async Task<string> AddCommentItem(CommentModel comment)
        {
            try
            {
                bool userhaveCommented = await UserHaveCommented(comment.MovieId, comment.UserId);
                if (!userhaveCommented)
                {
                    await context.SaveAsync(comment);
                }
                return Constants.SUCCESS;
            }
            catch (Exception ex)
            {
                return $"{Constants.ERROR} while adding comment data: {ex.Message}";
            }
        }

        /// <summary>
        /// This method assumes that the parameter for "movieId" recieves prefix + movieId
        /// </summary>
        /// <param name="commentMovieId"></param>
        /// <param name="userId"></param>
        /// <returns>true if the user have commented the specified movie, or false otherwise.</returns>
        internal static async Task<bool> UserHaveCommented(string commentMovieId, string userId)
        {
            QueryFilter filter = new();
            filter.AddCondition(Constants.MOVIE_ID, QueryOperator.Equal, commentMovieId);
            filter.AddCondition(Constants.USER_ID, QueryOperator.Equal, userId);

            var queryConfig = new QueryOperationConfig
            {
                Filter = filter
            };

            var query = context.FromQueryAsync<CommentModel>(queryConfig);
            var comment = await query.GetRemainingAsync();
            return comment.Any();
        }

        internal static async Task<string> AddRatingItem(RatingModel rating)
        {
            try
            {
                bool userHasRated = await UserHasRated(rating.MovieId, rating.UserId);
                if (!userHasRated)
                {
                    MovieModel movie = await UpdateMovieRatingProperties(rating.MovieId, rating.Rating);
                    await context.SaveAsync(movie);
                }
                await context.SaveAsync(rating);
                return Constants.SUCCESS;
            }
            catch (Exception ex)
            {
                return $"{Constants.ERROR} while adding rating data: {ex.Message}";
            }
        }

        /// <summary>
        /// This method assumes that the parameter for "movieId" recieves prefix + movieId
        /// </summary>
        /// <param name="ratingMovieId"></param>
        /// <param name="userId"></param>
        /// <returns>true if the user has rated the specified movie, or false otherwise.</returns>
        internal static async Task<bool> UserHasRated(string ratingMovieId, string userId)
        {
            QueryFilter filter = new();
            filter.AddCondition(Constants.MOVIE_ID, QueryOperator.Equal, ratingMovieId);
            filter.AddCondition(Constants.USER_ID, QueryOperator.Equal, userId);

            var queryConfig = new QueryOperationConfig
            {
                Filter = filter
            };

            var query = context.FromQueryAsync<RatingModel>(queryConfig);
            var rating = await query.GetRemainingAsync();
            return rating.Any();
        }


        internal static async Task<MovieModel> UpdateMovieRatingProperties(string movieId, double rating)
        {
            movieId = movieId[(movieId.IndexOf(Constants.HASHTAG) + 1)..];

            try
            {
                var dbMovie = await GetMovieById(movieId);

                dbMovie.NumOfRatings++;
                dbMovie.AvgRating = ((dbMovie.AvgRating * (dbMovie.NumOfRatings - 1)) + rating) / dbMovie.NumOfRatings;
                return dbMovie;
            } catch (Exception ex)
            {
                Debug.WriteLine($"{Constants.ERROR} while updating movie rating properties. {ex.Message}");
                throw;
            }
        }

        internal static async Task<List<RatingModel>> GetAllRatingsByMovieIdAsync(string movieId)
        {
            QueryFilter filter = new(Constants.MOVIE_ID, QueryOperator.Equal, Constants.PREFIX_RATING + movieId);
            var query = context.FromQueryAsync<RatingModel>(new QueryOperationConfig { Filter = filter });
            return await query.GetRemainingAsync();
        }

        /* methods to be implemented */
        internal static async Task<List<CommentModel>> GetAllCommentsAsync(string movieId)
        {
            try
            {
                QueryFilter filter = new();
                filter.AddCondition(Constants.TYPE, ScanOperator.Equal, Constants.PREFIX_COMMENT);
                filter.AddCondition(Constants.MOVIE_ID, ScanOperator.Equal, Constants.PREFIX_COMMENT + movieId);

                QueryOperationConfig config = new()
                {
                    Filter = filter,
                    IndexName = Constants.GSI_TYPE_TIMESTAMP       
                };

                var query = context.FromQueryAsync<CommentModel>(config);
                var comments = await query.GetRemainingAsync();
                return comments;
            }
            catch (AmazonDynamoDBException ex) 
            {
                Debug.WriteLine($"Error getting comments in the last 24 hours: {ex.Message}");
                throw;
            }
        }

        public static async Task<List<RatingModel>> GetAllRatingsAsync(string movieId)
        {
            try
            {
                QueryFilter filter = new();
                filter.AddCondition(Constants.TYPE, ScanOperator.Equal, Constants.PREFIX_RATING);
                filter.AddCondition(Constants.MOVIE_ID, ScanOperator.Equal, Constants.PREFIX_RATING + movieId);

                QueryOperationConfig config = new()
                {
                    Filter = filter,
                    IndexName = Constants.GSI_TYPE_TIMESTAMP
                };

                var query = context.FromQueryAsync<RatingModel>(config);
                var ratings = await query.GetRemainingAsync();
                return ratings;

            } catch (AmazonDynamoDBException ex)
            {
                Debug.WriteLine($"Error getting Ratings in the last 24 hours: {ex.Message}");
                throw;
            }
        }

    }
}
