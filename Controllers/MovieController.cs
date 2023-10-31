using _301153142_301137955_Soto_Ko_Lab3.Areas.Identity.Data;
using _301153142_301137955_Soto_Ko_Lab3.AWS;
using _301153142_301137955_Soto_Ko_Lab3.Models;
using _301153142_301137955_Soto_Ko_Lab3.Models.Movie;
using _301153142_301137955_Soto_Ko_Lab3.Models.Review;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using System.Diagnostics;
using System.Linq.Expressions;

namespace _301153142_301137955_Soto_Ko_Lab3.Controllers
{
    [Authorize]
    public class MovieController : Controller
    {
        private readonly UserManager<CustomUser> _userManager;
        public MovieController(UserManager<CustomUser> userManager)
        {
            _userManager = userManager;
        }

        /* convert images in memory form to base64 string to display */
        public string ConvertToBase64(MemoryStream memoryStream)
        {
            byte[] bytes = memoryStream.ToArray();
            string base64String = Convert.ToBase64String(bytes);
            return base64String;
        }

        private async Task<string> GetThumbnailBase64(string s3Key)
        {
            MemoryStream thumbnailMemory = await S3Service.GetThumbnail(s3Key);
            return ConvertToBase64(thumbnailMemory);
        }

        // GET: Movie
        public async Task<ActionResult> Index(string? genre, double? minRate, double? maxRate)
        {
            IndexViewModel model = new();

            try
            {
                if ((genre == null || genre.Trim().Length==0) && minRate == null && maxRate == null)
                {
                    model.Movies = await DynamoDBService.GetAllMovies();
                }
                else
                {
                    // genre
                    List<MovieModel> moviesByGenre = null;
                    if (genre != null)
                    {
                        moviesByGenre = await DynamoDBService.GetMoviesByGenre(genre);
                    }

                    // rate
                    List<MovieModel> moviesByRating = null;
                    if (minRate != null || maxRate != null)
                    {
                        // set min, max default if not given
                        minRate = (minRate == null) ? 0 : minRate;
                        maxRate = (maxRate == null) ? 5 : maxRate;
                        moviesByRating = await DynamoDBService.GetMoviesByAvgRating((double)minRate, (double)maxRate);

                        // search with ratings only
                        if (moviesByGenre == null)
                        {
                            model.Movies = moviesByRating;
                        }
                        // combine results
                        else
                        {
                            model.Movies = (moviesByGenre.Where(genreItem =>
                                moviesByRating.Any(ratingItem => ratingItem.MovieId == genreItem.MovieId)
                            )).ToList();
                        }
                    }
                    else if (moviesByGenre != null)
                    {
                        // search with genres only
                        model.Movies = moviesByGenre;
                    }                                    
                }

                foreach(MovieModel movie in model.Movies)
                {
                    // get thumbnails
                    movie.ThumbnailBase64 = await GetThumbnailBase64(movie.ThumbnailS3Key);
                }
            }
            catch (Exception ex)
            {
                // generic error message if something fails
                return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = $"{Constants.ERROR} Failed to retrieve movies: {ex.Message}" });
            }
            return View(model);
        }

        // GET: Movie/Upload
        public ActionResult Upload()
        {
            return View(new UploadViewModel());
        }

        // POST: Movie/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Upload(UploadViewModel model)
        {
            try
            {
                string userId = _userManager.GetUserId(HttpContext.User);
                string movieId = Guid.NewGuid().ToString();
                MovieModel newMovie = new(movieId, userId, model.Movie.Title);

                string releaseDate = model.Movie.ReleasedDate.Trim();
                if (releaseDate.Length > 0)
                {
                    newMovie.ReleasedDate = releaseDate;
                }

                // set genre with selected genres
                foreach (string genre in model.SelectedGenres)
                {
                    newMovie.Genre += genre + Constants.COMMA_DELIMITER;
                }

                newMovie.Genre = newMovie.Genre.Substring(0, newMovie.Genre.Length - 2); // remove extra comma

                newMovie.Directors = model.Movie.Directors.ElementAt(0).Split(Constants.COMMA).Select(director => director.Trim()).ToList();

                // save and set s3 vid, thumbnail 
                IFormFile vidFile = model.Movie.Video;
                string vidKey = movieId + Constants.PERIOD + vidFile.ContentType.Substring(vidFile.ContentType.IndexOf(Constants.FORWARD_SLASH) + 1);
                string vidUploadResult = await S3Service.UploadMovie(vidKey, vidFile);

                if (vidUploadResult.Contains(Constants.ERROR))
                {
                    return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = vidUploadResult });
                }
                newMovie.VideoS3Key = vidKey;

                IFormFile thumbnailFile = model.Movie.Thumbnail;
                string thumbnailKey = movieId + Constants.PERIOD + thumbnailFile.ContentType.Substring(thumbnailFile.ContentType.IndexOf(Constants.FORWARD_SLASH) + 1);
                string thumbnailUploadResult = await S3Service.UploadThumbnail(thumbnailKey, thumbnailFile);

                if (thumbnailUploadResult.Contains(Constants.ERROR))
                {
                    return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = thumbnailUploadResult });
                }
                newMovie.ThumbnailS3Key = thumbnailKey;
                newMovie.ThumbnailContentType = thumbnailFile.ContentType;

                // add movie data item
                string movieUploadResult = await DynamoDBService.AddMovieItem(newMovie);
                if (movieUploadResult.Contains(Constants.ERROR))
                {
                    return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = movieUploadResult });
                }
                return RedirectToAction(nameof(Details), new { movieId = movieId });
            }
            catch (Exception ex)
            {
                return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = $"{Constants.ERROR} while uploading movie: {ex.Message}" });
            }
        }

        // GET: Movie/Details?movieId={movieId}
        public async Task<ActionResult> Details(string? movieId)
        {
            DetailsViewModel model = new();
            try
            {
                MovieModel movie = await DynamoDBService.GetMovieById(movieId);

                // get thumbnail
                movie.ThumbnailBase64 = await GetThumbnailBase64(movie.ThumbnailS3Key);

                model.Movie = movie;

                model.ReviewViewModel = new()
                {
                    Comments = await DynamoDBService.GetAllCommentsAsync(movieId),
                    Ratings = await DynamoDBService.GetAllRatingsAsync(movieId),
                };

                //verify if user has commented in last 24 hours
                var oneDayAgo = DateTime.UtcNow.AddHours(-24);

                foreach (CommentModel comment in model.ReviewViewModel.Comments)
                {
                    DateTime commentTime = DateTime.Parse(comment.Timestamp);
                    if (comment.UserId == _userManager.GetUserId(User))
                    {
                        model.IsAddBtnHidden = true;
                        if (commentTime >= oneDayAgo)
                        {
                            model.ReviewViewModel.IsEditBtnHidden?.Add(false);
                            // pass existing comment, rating
                            model.Comment = comment.Comment;
                            int idx = model.ReviewViewModel.Comments.IndexOf(comment);
                            model.Rating = model.ReviewViewModel.Ratings.ElementAt(idx).Rating;  
                        }
                        else
                        {
                            model.ReviewViewModel.IsEditBtnHidden?.Add(true);
                        }
                    } else
                    {
                        model.ReviewViewModel.IsEditBtnHidden?.Add(true);
                    }
                }

                if (model.ReviewViewModel.IsEditBtnHidden != null)
                {
                    foreach (var item in model.ReviewViewModel.IsEditBtnHidden)
                    {
                        Debug.WriteLine($"btnHidden value: {item}");
                    }
                }
        

                return View(model);
            }
            catch (ArgumentOutOfRangeException)
            {
                return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = $"{Constants.ERROR} while fetching movie details: Movie not found" });

            }
            catch (ArgumentNullException ex)
            {                
                return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = $"{Constants.ERROR} A null value was found that cannot be assigned. {ex.Message}" });
            }

        }

        // POST: Movie/Review
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Review(DetailsViewModel model, string? movieId)
        {
            try
            {
                movieId = movieId[(movieId.IndexOf(Constants.HASHTAG) + 1)..];

                RatingModel rating = new(movieId, _userManager.GetUserId(User), model.Rating);
                CommentModel comment = new(movieId, _userManager.GetUserId(User), model.Comment);

                string AddCommentResult = await DynamoDBService.AddCommentItem(comment);
                string AddRatingResult = await DynamoDBService.AddRatingItem(rating);

                if (AddCommentResult.Contains(Constants.ERROR))
                {
                    return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = AddCommentResult });
                }
                if (AddRatingResult.Contains(Constants.ERROR))
                {
                    return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = AddRatingResult });
                }

                return RedirectToAction("Details", new { movieId = movieId });
            }
            catch (Exception ex)
            {
                // Handle exceptions as appropriate
                return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = ex.Message });
            }
        }


        // GET: Movie/Download?movieId={movieId}
        // assuming movieId without prefix
        public async Task<FileContentResult> Download(string? movieId)
        {
            MovieModel movie = await DynamoDBService.GetMovieById(movieId);

            MemoryStream movieStream = await S3Service.GetMovie(movie.VideoS3Key);

            string ext = movie.VideoS3Key.Substring(movie.VideoS3Key.LastIndexOf(Constants.PERIOD)+1);
            return File(movieStream.ToArray(), $"video/{ext}", $"{movie.Title}{Constants.PERIOD}{ext}");
        }

        // GET: Movie/UserMovies
        public async Task<ActionResult> UserMovies()
        {
            UserMoviesViewModel model = new();
            try
            {
                var userId = _userManager.GetUserId(User);
                model.Movies = await DynamoDBService.GetMoviesByUserId(userId);
                
                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction(Constants.VIEW_ERROR, new {errorMessage = $"{Constants.ERROR} while getting the movies for update: {ex.Message}" });
            }
        }

        // GET: Movie/UpdateMovie?movieId={movieId}
        public async Task<ActionResult> UpdateMovie(string movieId)
        {
            if (string.IsNullOrWhiteSpace(movieId))
            {
                return NotFound($"Movie Object not Found.");
            }

            string userId = _userManager.GetUserId(HttpContext.User);
            MovieModel movieToUpdate = await DynamoDBService.GetMovieById(movieId);

            if (movieToUpdate.UserId != userId)
            {
                return Unauthorized(Constants.NOT_AUTHORIZED_MSG);
            }

            UpdateViewModel model = new()
            {
                Movie = movieToUpdate
            };

            model.Movie.ThumbnailBase64 = await GetThumbnailBase64(movieToUpdate.ThumbnailS3Key);

            //Asigning ignored data in MovieModel to session.
            HttpContext.Session.SetString(Constants.SESSION_THUMB64, model.Movie.ThumbnailBase64);
            HttpContext.Session.SetString(Constants.SESSION_CONTENT_TYPE, model.Movie.ThumbnailContentType);

            return View(Constants.VIEW_UPDATE, model);
        }

        // POST: Movie/DeleteMovie?movieId={movieId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMovie(string movieId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(movieId))
                {
                    return NotFound($"Movie Object not Found.");
                }

                string userId = _userManager.GetUserId(HttpContext.User);
                MovieModel movie = await DynamoDBService.GetMovieById(movieId);

                if (movie.UserId != userId)
                {
                    return Unauthorized(Constants.NOT_AUTHORIZED_MSG);
                }

                // Handle movie deletion
                string deleteResult = "";

                // Delete s3 bucket obj(vid, thumbnail) 
                string deleteS3MovieResult = await S3Service.DeleteMovie(movie.VideoS3Key);
                if (deleteS3MovieResult != Constants.SUCCESS)
                {
                    deleteResult = deleteS3MovieResult;
                }

                string deleteS3ThumbnailResult = await S3Service.DeleteThumbnail(movie.ThumbnailS3Key);
                if (deleteS3ThumbnailResult != Constants.SUCCESS)
                {
                    deleteResult = deleteS3ThumbnailResult;
                }

                // Delete comments, ratings items for the movie, and delete movie meta data item
                string deleteMovieDataResult = await DynamoDBService.DeleteMovie(movie);
                if (deleteMovieDataResult != Constants.SUCCESS)
                {
                    deleteResult = deleteMovieDataResult;
                }


                if (deleteResult.Contains(Constants.ERROR))
                {
                    TempData["ErrorMessage"] = deleteResult;
                }
                else
                {
                    TempData["SuccessMessage"] = "Movie deleted successfully!";
                }

                return RedirectToAction(nameof(UserMovies));
            } catch (Exception ex) {

                return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = $"{Constants.ERROR} while deleting movie: {ex.Message}" });

            }
        }

        // POST: Movie/UpdatePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePost(UpdateViewModel model)
        {
            try
            {
                if (model == null || model.Movie == null)
                {
                    return BadRequest("Movie Object is required");
                }

                if(model.Movie.UserId != _userManager.GetUserId(HttpContext.User))
                {
                    return Unauthorized(Constants.NOT_AUTHORIZED_MSG);
                }

                string thumbnailBase64 = HttpContext.Session.GetString(Constants.SESSION_THUMB64);
                string thumbnailContentType = HttpContext.Session.GetString(Constants.SESSION_CONTENT_TYPE);

                //assigning the thumbnail data to model
                model.Movie.ThumbnailBase64 = thumbnailBase64;
                model.Movie.ThumbnailContentType = thumbnailContentType;

                // set genre with selected genres
                model.Movie.Genre = model.ConvertSelectedGenresToString();
                //converting directors to array
                model.Movie.Directors = model.Movie.Directors.ElementAt(0)!.Split(Constants.COMMA).Select(director => director.Trim()).ToList()!;
                // adding the user ID value before processing the update
                var userId = _userManager.GetUserId(User);
                model.Movie.UserId = userId;
             
                await DynamoDBService.UpdateMovie(model.Movie);

                // Clear session data after the update
                HttpContext.Session.Remove(Constants.SESSION_THUMB64);
                HttpContext.Session.Remove(Constants.SESSION_CONTENT_TYPE);

                TempData["SuccessMessage"] = "Movie updated successfully!";
                return View(Constants.VIEW_UPDATE, model); // Return mode for displaying again the Update page with movie editable data
            }
            catch (Exception ex)
            {
                return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = $"{Constants.ERROR} Updating the movie with ID: {model.Movie.MovieId} -- {ex.Message}" });
            }
        }


        // Error action to display errors in Error page.
        public IActionResult Error(string? errorMessage)
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = errorMessage!
            };

            return View(errorViewModel);
        }

    }
}
