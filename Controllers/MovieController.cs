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
                        maxRate = (maxRate == null) ? 10 : maxRate;
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
                    newMovie.Genre += genre + Constants.COMMA;
                }

                newMovie.Genre = newMovie.Genre.Substring(0, newMovie.Genre.Length - 1); // remove extra comma

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

                // add updatedMovie data item
                string movieUploadResult = await DynamoDBService.AddMovieItem(newMovie);
                if (movieUploadResult.Contains(Constants.ERROR))
                {
                    return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = movieUploadResult });
                }
                return RedirectToAction(nameof(Details), new { movieId = movieId });
            }
            catch (Exception ex)
            {
                return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = $"{Constants.ERROR} while uploading updatedMovie: {ex.Message}" });
            }
        }

        // GET: Movie/Details?movieId={movieId}

        public async Task<ActionResult> Details(string? movieId)
        {
            DetailsViewModel model = new();
            try
            {
                MovieModel movie = await DynamoDBService.GetMovieById(movieId);
                model.Movie = movie;

                model.ReviewViewModel = new()
                {
                    Comments = await DynamoDBService.GetAllCommentsAsync(movieId),
                    Ratings = await DynamoDBService.GetAllRatingsAsync(movieId),
                };

                //verify if user has commented in last 24 hours
                var oneDayAgo = DateTime.UtcNow.AddHours(-24);

                foreach (var comment in model.ReviewViewModel.Comments)
                {
                    DateTime commentTime = DateTime.Parse(comment.Timestamp);
                    if (commentTime <= oneDayAgo && comment.UserId == _userManager.GetUserId(User))
                    {
                        model.ReviewViewModel.IsEditBtnHidden?.Add(false);
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

        //Movie/Review
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

        //GET: Movie/UserMovies
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

        //Syncronous
        //Movie/UpdateGet
        public ActionResult UpdateGet(UpdateViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return NotFound($"Movie Object not Found.");
                }
               
            }
            catch (Exception ex)
            {
                return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = $"{Constants.ERROR} while getting the updatedMovie update: {ex.Message}" });
            }
            return View(Constants.VIEW_UPDATE, model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        //Movie/UpdatePost
        public async Task<ActionResult> UpdatePost(UpdateViewModel model)
        {
            try
            {

                // set genre with selected genres
                model.Movie.Genre = model.ConvertSelectedGenresToString();
                //converting directors to array
                model.Movie.Directors = model.Movie.Directors.ElementAt(0)!.Split(Constants.COMMA).Select(director => director.Trim()).ToList()!;
                //adding the user ID value before processing the update
                var userId = _userManager.GetUserId(User);
                model.Movie.UserId = userId;

                if (model == null)
                {
                    return BadRequest("Movie Object is required");
                }
             
                await DynamoDBService.UpdateMovie(model.Movie);

                TempData["SuccessMessage"] = "Movie updated successfully!";
                return View(Constants.VIEW_UPDATE, model); // Return mode for displaying again the Update page with movie editable data
            }
            catch (Exception ex)
            {
                return RedirectToAction(Constants.VIEW_ERROR, new { errorMessage = $"{Constants.ERROR} Updating the updatedMovie with ID: {model.Movie.MovieId} -- {ex.Message}" });
            }
        }

        // GET: Movie/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Movie/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        //Error action to display errors in Error page.
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
