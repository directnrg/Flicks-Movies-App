//using _301153142_301137955_Soto_Ko_Lab3.Areas.Identity.Data;
//using _301153142_301137955_Soto_Ko_Lab3.AWS;
//using _301153142_301137955_Soto_Ko_Lab3.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using System.Diagnostics;

//namespace _301153142_301137955_Soto_Ko_Lab3.Views.Movie
//{
//    [Authorize]
//    public class IndexModel : PageModel
//    {
//        private readonly UserManager<CustomUser>
//            _userManager;

//        public List<MovieModel> Movies { get; set; }

//        public IndexModel(
//            UserManager<CustomUser> userManager)
//        {
//            _userManager = userManager;
//            Movies = new List<MovieModel>();
//        }

//        public async Task<IActionResult> OnGetAsync()
//        {
//            Debug.WriteLine("Inside OnGetAsync");
//            // get movies from dynamodb
//            Movies = await DynamoDBService.GetAllMovies();

//            return Page();
//        }
//    }
//}
