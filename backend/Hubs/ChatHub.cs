using Microsoft.AspNetCore.SignalR;
using backend.Models;
using backend.Data;

namespace backend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        // When user connects, add to role group
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var role = httpContext?.Request.Query["role"].ToString();

            if (!string.IsNullOrEmpty(role))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, role.ToLower());
            }

            await base.OnConnectedAsync();
        }

        
    }
}
