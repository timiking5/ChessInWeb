using ChessInWeb.Components;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ChessInWeb.Controllers;

[Authorize]
public class GameController : Controller
{
    public static Dictionary<long, Game> GamesDictionary = new();
    public static List<Game> AwaitingGames = new();
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public GameController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

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
        Random rand = new Random();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        game.GameManager = new();
        if (rand.NextDouble() >= 0.5)
        {
            game.WhitePlayer = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
            game.WhitePlayerId = userId;
        }
        else
        {
            game.BlackPlayer = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
            game.BlackPlayerId = userId;
        }
        _unitOfWork.Game.Create(game);
        _unitOfWork.Save();
        AwaitingGames.Add(game);
        return RedirectToAction("ChessBoard", new { game.Id });
    }
    public IActionResult StartGame(long id)
    {
        return RedirectToAction("ChessBoard");
    }
    public IActionResult ChessBoard(long id)
    {
        
        if (GamesDictionary.ContainsKey(id) || AwaitingGames.Where(x => x.Id == id).Any())
        {
            Game game;
            game = GamesDictionary.TryGetValue(id, out game) ? game : AwaitingGames.Where(x => x.Id == id).First();
            return View(game);
        }
        return NotFound();
    }
}
