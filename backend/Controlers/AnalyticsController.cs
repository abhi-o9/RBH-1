using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using backend.Services;
using backend.Models;
using backend.Data;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly CouchDbService _couchDb;
    private readonly ApplicationDbContext _context;

    public AnalyticsController(
        CouchDbService couchDb,
        ApplicationDbContext context)
    {
        _couchDb = couchDb;
        _context = context;
    }

    // ================= ADMIN =================

    [HttpGet("user-growth")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetUserGrowth()
    {
        var users = await _couchDb.GetAllAsync<User>();

        var growthData = users
            .Where(u => u.status == "approved" && u.approvedAt.HasValue)
            .GroupBy(u => u.approvedAt!.Value.Date)
            .Select(g => new
            {
                date = g.Key.ToString("yyyy-MM-dd"),
                count = g.Count()
            })
            .OrderBy(x => x.date)
            .ToList();

        return Ok(growthData);
    }

    [HttpGet("users-by-role")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetUsersByRole()
    {
        var users = await _couchDb.GetAllAsync<User>();

        var roleData = users
            .Where(u => u.status == "approved")
            .GroupBy(u => u.role.ToLower())
            .Select(g => new
            {
                role = g.Key,
                count = g.Count()
            })
            .ToList();

        return Ok(roleData);
    }

    // ================= USER =================

    [HttpGet("my-messages-per-day")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> GetMyMessagesPerDay()
    {
        var username = User.Identity?.Name;

        // Step 1: Fetch only required messages from DB
        var messages = await _context.Messages
            .Where(m => m.SenderId == username)
            .ToListAsync();

        // Step 2: Group in memory
        var result = messages
            .GroupBy(m => m.Timestamp.Date)
            .Select(g => new
            {
                date = g.Key.ToString("yyyy-MM-dd"),
                count = g.Count()
            })
            .OrderBy(x => x.date)
            .ToList();

        return Ok(result);
    }


    [HttpGet("my-activity-trend")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> GetMyActivityTrend()
    {
        var username = User.Identity?.Name;
        var userRole = "user";

        var messages = await _context.Messages
            .Where(m =>
                m.SenderId == username ||
                (
                    m.SenderId != username &&
                    (m.ReceiverRole.ToLower() == "all" ||
                     m.ReceiverRole.ToLower() == userRole)
                )
            )
            .ToListAsync();

        var result = messages
            .GroupBy(m => m.Timestamp.Date)
            .Select(g => new
            {
                date = g.Key.ToString("yyyy-MM-dd"),
                sent = g.Count(x => x.SenderId == username),
                received = g.Count(x =>
                    x.SenderId != username &&
                    (x.ReceiverRole.ToLower() == "all" ||
                     x.ReceiverRole.ToLower() == userRole)
                )
            })
            .OrderBy(x => x.date)
            .ToList();

        return Ok(result);
    }


}
