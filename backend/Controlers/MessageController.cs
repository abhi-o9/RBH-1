using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using backend.Data;
using backend.Models;
using backend.Services;
using backend.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
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
        public async Task<IActionResult> SendMessage([FromBody] string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                return BadRequest("Message cannot be empty.");

            // Get logged-in user (from JWT)
            var sender = User.Identity?.Name ?? "Unknown";

            // Hash
            var hash = _rsaService.GenerateHash(messageText);

            // Encrypt
            var encryptedMessage = _rsaService.Encrypt(messageText);

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = sender,
                ReceiverRole = "All",
                EncryptedMessage = encryptedMessage,
                MessageHash = hash,
                Timestamp = DateTime.UtcNow
            };

            // Save to PostgreSQL
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Broadcast decrypted message to clients
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", sender, messageText);

            return Ok("Message saved and broadcasted successfully.");
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetMessageHistory()
        {
            var user = User.Identity?.Name ?? "Unknown";

            var messages = _context.Messages
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
