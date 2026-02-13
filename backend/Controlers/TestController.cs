using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddTestMessage()
        {
            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = "TestUser",
                ReceiverRole = "Admin",
                EncryptedMessage = "TestEncryptedData",
                MessageHash = "TestHash",
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok("Message Saved Successfully!");
        }
    }
}
