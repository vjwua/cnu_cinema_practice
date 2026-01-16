using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
// using cnu_cinema_practice.Models;

namespace cnu_cinema_practice.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // ...
}