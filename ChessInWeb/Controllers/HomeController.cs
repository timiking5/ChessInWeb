global using ChessInWeb.Models;
global using ChessLogic;
global using Models;
global using Microsoft.AspNetCore.Mvc;
global using System.Diagnostics;
global using Microsoft.AspNetCore.Authorization;

namespace ChessInWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        GameManager manager = new();
        return View(manager);
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