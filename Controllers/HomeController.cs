﻿using Flicks_App.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Flicks_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // redirect to search (index) page of Movie controller
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Movie");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}