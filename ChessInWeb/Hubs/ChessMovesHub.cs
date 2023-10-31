using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Identity.Client;

namespace ChessInWeb.Hubs;

public class ChessMovesHub : Hub
{
    public static Dictionary<long, List<string>> gameToConnections = new();
    public async Task SendMove(long gameId, int startIndex, int endIndex, int moveFlag)
    {
        if (gameToConnections.ContainsKey(gameId))
        {
            gameToConnections[gameId].ForEach(connId =>
            {
                Clients.Client(connId).SendAsync("RecieveMove", startIndex, endIndex, moveFlag).GetAwaiter().GetResult();
            });
        }
    }
    public Task AddConnectionToGame(long gameId)
    {
        if (gameToConnections.ContainsKey(gameId))
        {
            gameToConnections[gameId].Add(Context.ConnectionId);
            return Task.CompletedTask;
        }
        gameToConnections[gameId] = new() { Context.ConnectionId };
        return Task.CompletedTask;
    }
    public Task RemoveConnectionFromGame(long gameId)
    {
        try
        {
            gameToConnections[gameId].Remove(Context.ConnectionId);
        }
        catch (Exception)
        {
            // maybe log exception in future
        }
        return Task.CompletedTask;
    }
    
}
