namespace ChessInWeb.Service;

public class GamesStorage
{
    public static readonly Dictionary<long, Game> GamesDictionary = new();
    public static readonly List<Game> AwaitingGames = new();
    public static void AddGame(Game game)
    {
        AwaitingGames.Add(game);
        GamesDictionary[game.Id] = game;
    }
    public static void RemoveGame(Game game)
    {
        GamesDictionary.Remove(game.Id);
    }
    public static void RemoveAwaitingGame(Game game)
    {
        AwaitingGames.Remove(game);
    }
    public static void RemoveAwaitingGame(long gameId)
    {
        var game = AwaitingGames.Where(x => x.Id == gameId).FirstOrDefault();
        if (game is not null)
        {
            AwaitingGames.Remove(game);
        }
    }
}
