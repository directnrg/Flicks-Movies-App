using _301153142_301137955_Soto_Ko_Lab3.AWS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _301153142_301137955_Soto_Ko_Lab3.Models.Movie
{
    public class IndexModel 
    {
        public List<MovieModel> Movies { get; set; }
        
        public IndexModel()
        {
            Movies = new List<MovieModel>();
        }
    }
}
