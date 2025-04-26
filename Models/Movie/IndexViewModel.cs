using Flicks_App.AWS;
using Flicks_App.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flicks_App.Models.Movie
{
    public class IndexViewModel 
    {
        public List<MovieModel> Movies { get; set; }
        public MovieGenre Genre { get; set; }
    }
}
