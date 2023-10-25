global using Microsoft.AspNetCore.SignalR;

namespace ChessInWeb.Hubs;

public class ChatHub : Hub
{
    public Task SendMessage(string message, string user)
    {
        return Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
