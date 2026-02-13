using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Services;
using backend.Models;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly CouchDbService _couchDb;

    public AdminController(CouchDbService couchDb)
    {
        _couchDb = couchDb;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingUsers()
    {
        var users = await _couchDb.GetAllAsync<User>();

        var pendingUsers = users
            .Where(u => u.status == "pending")
            .Select(u => new
            {
                u._id,
                u.firstName,
                u.lastName,
                u.email,
                u.role,
                u.status
            });

        return Ok(pendingUsers);
    }

    [HttpPut("approve/{id}")]
    public async Task<IActionResult> ApproveUser(string id)
    {
        var user = await _couchDb.GetAsync<User>(id);

        if (user == null)
            return NotFound("User not found");

        user.status = "approved";

        await _couchDb.UpdateAsync(user._id, user);

        return Ok("User approved successfully");
    }
}
