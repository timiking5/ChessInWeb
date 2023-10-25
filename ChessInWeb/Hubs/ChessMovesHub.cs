using Microsoft.Identity.Client;

namespace ChessInWeb.Hubs;

public class ChessMovesHub : Hub
{
    public static Dictionary<string, List<string>> gameToUsers = new();
    public Task SendMove(string gameId, Move move)
    {
        return Task.CompletedTask;
    }
    public static void AddUserToGame(string userId, string gameId)
    {
        if (gameToUsers.ContainsKey(gameId))
        {
            gameToUsers[gameId].Add(userId);
            return;
        }
        gameToUsers[gameId] = new() { userId };
    }
    public static void RemoveUserFromGame(string userId, string gameId)
    {
        gameToUsers[gameId].Remove(userId);
        if (gameToUsers[gameId].Count == 0)
        {
            gameToUsers.Remove(gameId);
        }
    }
}
