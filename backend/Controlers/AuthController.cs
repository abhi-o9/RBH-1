using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Services;
using backend.Models;
using System.Security.Cryptography;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly CouchDbService _couchDb;

    public AuthController(IConfiguration config, CouchDbService couchDb)
    {
        _config = config;
        _couchDb = couchDb;
    }

    // ---------------- LOGIN (TEMP ADMIN) ----------------
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password required");

        var email = request.Username.ToLower().Trim();

        var users = await _couchDb.GetAllAsync<User>();

        var user = users.FirstOrDefault(u =>
            string.Equals(u.email, email, StringComparison.OrdinalIgnoreCase)
        );

        if (user == null)
            return Unauthorized("User not found");

        var enteredPasswordHash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(request.Password))
        );

        if (user.passwordHash != enteredPasswordHash)
            return Unauthorized("Invalid credentials");

        if (!string.Equals(user.status, "approved", StringComparison.OrdinalIgnoreCase))
            return Unauthorized("Waiting for admin approval");

        // Generate new session ID
        var newSessionId = Guid.NewGuid().ToString();

        // Save session ID to user
        user.currentSessionId = newSessionId;

        // Update user document in CouchDB
        await _couchDb.UpdateAsync(user._id, user);


        var claims = new[]
        {
        new Claim(ClaimTypes.Name, user.email),
        new Claim(ClaimTypes.Role, user.role),
        new Claim("sessionId", newSessionId)
    };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(
                int.Parse(_config["Jwt:ExpiresInMinutes"]!)
            ),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256
            )
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            role = user.role
        });
    }


    // ---------------- REGISTER (COUCHDB) ----------------
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.email) ||
            string.IsNullOrWhiteSpace(request.password))
            return BadRequest("Email and password required");

        var email = request.email.ToLower().Trim();
        var userId = $"user:{email}";

        // Check if user already exists
        try
        {
            var existingUser = await _couchDb.GetAsync<User>(userId);
            if (existingUser != null)
                return Conflict("User already exists.");
        }
        catch
        {
            // If not found, continue
        }

        var passwordHash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(request.password))
        );

        var user = new User
        {
            _id = userId,
            firstName = request.firstName,
            lastName = request.lastName,
            email = email,
            passwordHash = passwordHash,
            role = request.role.ToLower(),
            status = "pending",
            registeredAt = DateTime.UtcNow,
            approvedAt = null
        };

        await _couchDb.CreateAsync(userId,user);

        return Ok("Registration successful. Awaiting admin approval.");
    }


    // ---------------- MODELS ----------------
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
