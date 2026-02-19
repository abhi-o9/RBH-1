using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Hubs;
using System.Security.Cryptography;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// RsaEncryptionServices
builder.Services.AddSingleton<RsaEncryptionService>();

// SignalR Hubs
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    // 🔥 REQUIRED FOR SIGNALR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/chathub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});


// CouchDB
builder.Services.AddHttpClient<CouchDbService>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var couchService = scope.ServiceProvider.GetRequiredService<CouchDbService>();
    await couchService.EnsureAdminExistsAsync();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();

app.UseAuthorization();
app.UseMiddleware<SessionValidationMiddleware>();
app.MapHub<ChatHub>("/chathub");
app.MapControllers();
app.Run();
