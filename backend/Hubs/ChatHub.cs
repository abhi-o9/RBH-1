using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs
{
    public class ChatHub : Hub
    {
        // This method will be called when a message is sent
        public async Task SendMessage(string sender, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", sender, message);
        }
    }
}
