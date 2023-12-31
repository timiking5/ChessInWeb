﻿using ChessInWeb.Components;
using ChessInWeb.Hubs;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;
namespace ChessInWeb.Controllers;

[Authorize]
public class GameController : Controller
{
    public static Dictionary<long, Game> GamesDictionary = new();
    public static List<Game> AwaitingGames = new();
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<LobbyHub> _lobbyHub;
    private readonly HubConnection _chessHubConnection;  

    public GameController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork,
        IHubContext<LobbyHub> lobbyHub)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _lobbyHub = lobbyHub;
        _chessHubConnection = new HubConnectionBuilder()
        .WithUrl("https://localhost:7023/chessmoveshub")
        .WithAutomaticReconnect()
        .Build();
        _chessHubConnection.StartAsync().GetAwaiter().GetResult();
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
        GamesDictionary[game.Id] = game;

        _lobbyHub.Clients.All.SendAsync("ReceiveGame").GetAwaiter().GetResult();
        return RedirectToAction("ChessBoard", new { game.Id });
    }
    public IActionResult StartGame(long id)
    {
        var game = AwaitingGames.Where(x => x.Id == id).FirstOrDefault();
        if (game is null)
        {
            RedirectToAction("Index", "Home");
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (game.WhitePlayerId is null)
        {
            game.WhitePlayerId = userId;
        }
        else
        {
            game.BlackPlayerId = userId;
        }
        AwaitingGames.Remove(game);
        GamesDictionary[game.Id] = game;
        _lobbyHub.Clients.All.SendAsync("ReceiveGame").GetAwaiter().GetResult();
        if (_chessHubConnection is not null)
        {
            _chessHubConnection.SendAsync("SendMove", game.Id, -1, -1, -1).GetAwaiter().GetResult();
        }
        return RedirectToAction("ChessBoard", new { id });
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
    public IActionResult FinishGame(long gameId, string winnerId)
    {
        if (!GamesDictionary.ContainsKey(gameId))
        {
            return RedirectToAction("ChessBoard", new { gameId });
        }
        var game = GamesDictionary[gameId];
        game.WinnerId = winnerId;
        _unitOfWork.Game.Update(game);
        _unitOfWork.Save();
        return RedirectToAction("Index");
    }
    #region STATIC CALLS
    public static void MakeMoveOnBoard(long gameId, Move move)
    {
        if (!GamesDictionary.ContainsKey(gameId))
        {
            return;
        }
        GamesDictionary[gameId].GameManager.MakeMove(move);
    }
    #endregion
}
