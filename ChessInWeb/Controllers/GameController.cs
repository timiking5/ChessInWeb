using ChessInWeb.Components;

namespace ChessInWeb.Controllers;

[Authorize]
public class GameController : Controller
{
    public static Dictionary<int, Game> GamesDictionary = new();
    public static List<Game> AwaitingGames = new();
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Create(Game game)
    {
        return View();
    }
    public IActionResult StartGame(long id)
    {
        return RedirectToAction("ChessBoard");
    }
    public IActionResult ChessBoard(long id)
    {
        return View();
    }
}
