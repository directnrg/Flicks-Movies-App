using _301153142_301137955_Soto_Ko_Lab3.Areas.Identity.Data;
using _301153142_301137955_Soto_Ko_Lab3.AWS;
using _301153142_301137955_Soto_Ko_Lab3.Models;
using _301153142_301137955_Soto_Ko_Lab3.Models.Movie;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
                return RedirectToAction("Error", new { errorMessage = $"{Constants.ERROR} Failed to retrieve movies: {ex.Message}" });
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
                    return RedirectToAction("Error", new { errorMessage = vidUploadResult });
                }
                newMovie.VideoS3Key = vidKey;

                IFormFile thumbnailFile = model.Movie.Thumbnail;
                string thumbnailKey = movieId + Constants.PERIOD + thumbnailFile.ContentType.Substring(thumbnailFile.ContentType.IndexOf(Constants.FORWARD_SLASH) + 1);
                string thumbnailUploadResult = await S3Service.UploadThumbnail(thumbnailKey, thumbnailFile);

                if (thumbnailUploadResult.Contains(Constants.ERROR))
                {
                    return RedirectToAction("Error", new { errorMessage = thumbnailUploadResult });
                }
                newMovie.ThumbnailS3Key = thumbnailKey;

                // add updatedMovie data item
                string movieUploadResult = await DynamoDBService.AddMovieItem(newMovie);
                if (movieUploadResult.Contains(Constants.ERROR))
                {
                    return RedirectToAction("Error", new { errorMessage = movieUploadResult });
                }
                return RedirectToAction(nameof(Details), new { movieId = movieId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { errorMessage = $"{Constants.ERROR} while uploading updatedMovie: {ex.Message}" });
            }
        }

        // GET: Movie/Details?movieId={movieId}
        // assuming movieId without prefix
        public async Task<ActionResult> Details(string? movieId)
        {
            DetailsViewModel model = new();
            model.Movie = new MovieModel();
            try
            {
                MovieModel movie = await DynamoDBService.GetMovieById(movieId);
                model.Movie = movie;
            }
            catch (ArgumentOutOfRangeException)
            {
                return RedirectToAction("Error", new { errorMessage = $"{Constants.ERROR} while loading updatedMovie details: Movie not found" });

            }
            catch (Exception ex)
            {                
                return RedirectToAction("Error", new { errorMessage = $"{Constants.ERROR} while loading updatedMovie details: {ex.Message}" });
            }
            return View(model);
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
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new {errorMessage = $"{Constants.ERROR} while getting the movies for update: {ex.Message}" });
            }

            return View(model);
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
                return RedirectToAction("Error", new { errorMessage = $"{Constants.ERROR} while getting the updatedMovie update: {ex.Message}" });
            }
            return View("Update", model);
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
                return View("Update", model); // Return mode for displaying again the Update page with movie editable data
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { errorMessage = $"{Constants.ERROR} Updating the updatedMovie with ID: {model.Movie.MovieId} -- {ex.Message}" });
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
