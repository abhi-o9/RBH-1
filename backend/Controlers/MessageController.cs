using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using backend.Data;
using backend.Models;
using backend.Services;
using backend.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    public class SendMessageRequest
    {
        public string Message { get; set; } = string.Empty;
        public string ReceiverRole { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Only logged-in users can send messages
    public class MessageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly RsaEncryptionService _rsaService;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageController(
            ApplicationDbContext context,
            RsaEncryptionService rsaService,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _rsaService = rsaService;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message cannot be empty.");

            var sender = User.Identity?.Name ?? "Unknown";
            var senderRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? "user";

            var receiverRole = request.ReceiverRole.ToLower();

            // Hash + Encrypt
            var hash = _rsaService.GenerateHash(request.Message);
            var encryptedMessage = _rsaService.Encrypt(request.Message);

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = sender,
                SenderRole = senderRole,
                ReceiverRole = receiverRole,
                EncryptedMessage = encryptedMessage,
                MessageHash = hash,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // 🔥 Broadcast correctly
            if (receiverRole == "all")
            {
                await _hubContext.Clients.All
                    .SendAsync("ReceiveMessage", sender, request.Message);
            }
            else
            {
                // Send to receiver role group
                await _hubContext.Clients.Group(receiverRole)
                    .SendAsync("ReceiveMessage", sender, request.Message);

                // ALSO send to sender's role group
                await _hubContext.Clients.Group(senderRole.ToLower())
                    .SendAsync("ReceiveMessage", sender, request.Message);
            }



            return Ok("Message sent successfully.");
        }


        [HttpGet("history")]
        public async Task<IActionResult> GetMessageHistory()
        {
            var userRole = User.Claims
                .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                ?.Value?.ToLower() ?? "user";

            var messages = _context.Messages
                .Where(m => m.ReceiverRole == "all" || m.ReceiverRole == userRole)
                .OrderBy(m => m.Timestamp)
                .ToList();

            var decryptedMessages = messages.Select(m => new
            {
                m.Id,
                m.SenderId,
                m.ReceiverRole,
                Message = _rsaService.Decrypt(m.EncryptedMessage),
                m.Timestamp
            });

            return Ok(decryptedMessages);
        }

    }
}
