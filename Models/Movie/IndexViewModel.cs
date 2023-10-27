using _301153142_301137955_Soto_Ko_Lab3.AWS;
using _301153142_301137955_Soto_Ko_Lab3.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _301153142_301137955_Soto_Ko_Lab3.Models.Movie
{
    public class IndexViewModel 
    {
        public List<MovieModel> Movies { get; set; }
        public MovieGenre Genre { get; set; }
        
    }
}
