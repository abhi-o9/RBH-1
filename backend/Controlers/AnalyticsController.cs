using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Services;
using backend.Models;

[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "admin")]
public class AnalyticsController : ControllerBase
{
    private readonly CouchDbService _couchDb;

    public AnalyticsController(CouchDbService couchDb)
    {
        _couchDb = couchDb;
    }

    [HttpGet("user-growth")]
    public async Task<IActionResult> GetUserGrowth()
    {
        var users = await _couchDb.GetAllAsync<User>();

        var growthData = users
            .Where(u =>
                u.status == "approved" &&
                u.approvedAt.HasValue
            )
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
}
