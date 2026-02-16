using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly RsaEncryptionService _rsaService;

        public TestController(ApplicationDbContext context, RsaEncryptionService rsaService)
        {
            _context = context;
            _rsaService = rsaService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddTestMessage()
        {
            string plainMessage = "Hello Admin Securely";

            // Generate Hash
            var hash = _rsaService.GenerateHash(plainMessage);

            // Encrypt Message
            var encryptedMessage = _rsaService.Encrypt(plainMessage);

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = "TestUser",
                ReceiverRole = "Admin",
                EncryptedMessage = encryptedMessage,
                MessageHash = hash,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok("Encrypted Message Saved Successfully!");
        }

        [HttpGet("latest")]
        public IActionResult GetLatestMessage()
        {
            var latestMessage = _context.Messages
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefault();

            if (latestMessage == null)
                return NotFound("No messages found.");

            var decryptedText = _rsaService.Decrypt(latestMessage.EncryptedMessage);

            return Ok(new
            {
                Encrypted = latestMessage.EncryptedMessage,
                Decrypted = decryptedText,
                Hash = latestMessage.MessageHash
            });
        }

    }
}