global using Microsoft.AspNetCore.SignalR;

namespace ChessInWeb.Hubs;

public class LobbyHub : Hub
{
    public Task SendMessage(string message, string user)
    {
        return Clients.All.SendAsync("ReceiveMessage", user, message);
    }
    public Task SendGame()
    { // we don't need params, we just need lobby to rerender its component
        return Clients.All.SendAsync("ReceiveGame");
    }
}
