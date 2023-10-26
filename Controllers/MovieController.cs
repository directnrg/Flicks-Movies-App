using _301153142_301137955_Soto_Ko_Lab3.Areas.Identity.Data;
using _301153142_301137955_Soto_Ko_Lab3.AWS;
using _301153142_301137955_Soto_Ko_Lab3.Models;
using _301153142_301137955_Soto_Ko_Lab3.Models.Movie;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            catch (Exception)
            {
                // generic error message if something fails
                return View("Error", new ErrorViewModel { ErrorMessage = $"Failed to retrieve movies." });
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
                MovieModel newMovie = new (movieId, userId, model.Movie.Title);

                string releaseDate = model.Movie.ReleasedDate.Trim();
                if (releaseDate.Length > 0)
                {
                    newMovie.ReleasedDate = releaseDate;
                }

                string genre = model.Movie.Genre.Trim();
                if (genre.Length > 0)
                {
                    newMovie.Genre = genre;
                }

                newMovie.Directors = model.Movie.Directors.ElementAt(0).Split(Constants.COMMA).Select(director => director.Trim()).ToList();

                // save and set s3 vid, thumbnail 
                IFormFile vidFile = model.Movie.Video;
                string vidKey = movieId + Constants.PERIOD + vidFile.ContentType.Substring(vidFile.ContentType.IndexOf(Constants.FORWARD_SLASH)+1);
                string vidUploadResult = await S3Service.UploadMovie(vidKey, vidFile);

                if (vidUploadResult.Contains(Constants.ERROR))
                {
                    model.Message = vidUploadResult;
                    return View(model);
                }
                newMovie.VideoS3Key = vidKey;

                IFormFile thumbnailFile = model.Movie.Thumbnail;
                string thumbnailKey = movieId + Constants.PERIOD + thumbnailFile.ContentType.Substring(thumbnailFile.ContentType.IndexOf(Constants.FORWARD_SLASH) + 1);
                string thumbnailUploadResult = await S3Service.UploadThumbnail(thumbnailKey, thumbnailFile);

                if (thumbnailUploadResult.Contains(Constants.ERROR))
                {
                    model.Message = thumbnailUploadResult;
                    return View(model);
                }
                newMovie.ThumbnailS3Key = thumbnailKey;

                // add movie data item
                string movieUploadResult = await DynamoDBService.AddMovieItem(newMovie);
                if (movieUploadResult.Contains(Constants.ERROR))
                {
                    model.Message = movieUploadResult;
                    return View(model);
                }
                return RedirectToAction(nameof(Details), new { movieId = movieId });
            }
            catch (Exception ex)
            {
                model.Message = $"{Constants.ERROR} while uploading movie: {ex.Message}";
                return View(model);
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
                model.Message = $"{Constants.ERROR} while loading movie details: Movie not found";
            }
            catch (Exception ex)
            {                
                model.Message = $"{Constants.ERROR} while loading movie details: {ex.Message}";
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

        // GET: Movie/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Movie/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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
    }
}
