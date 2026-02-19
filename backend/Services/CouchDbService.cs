    using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using backend.Models;
using System.Security.Cryptography;
using System.Text.Json.Serialization;


namespace backend.Services;

public class CouchDbService
{
    private readonly HttpClient _httpClient;
    private readonly string _database;

    public CouchDbService(HttpClient httpClient, IConfiguration configuration)
    {
        var baseUrl = configuration["CouchDb:BaseUrl"]!;
        var username = configuration["CouchDb:Username"]!;
        var password = configuration["CouchDb:Password"]!;
        _database = configuration["CouchDb:Database"]!;

        httpClient.BaseAddress = new Uri(baseUrl);

        var auth = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{username}:{password}")
        );

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", auth);

        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );

        _httpClient = httpClient;
    }

    public async Task CreateAsync<T>(string id, T document)
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(document, options);

        // Remove _id from JSON when using PUT with explicit id
        var jsonDoc = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        jsonDoc.Remove("_id");

        var finalJson = JsonSerializer.Serialize(jsonDoc);
        var content = new StringContent(finalJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"/{_database}/{id}", content);
        response.EnsureSuccessStatusCode();
    }


    public async Task<T?> GetAsync<T>(string id)
    {
        var response = await _httpClient.GetAsync($"/{_database}/{id}");
        if (!response.IsSuccessStatusCode) return default;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task<List<T>> GetAllAsync<T>()
    {
        var response = await _httpClient.GetAsync($"/{_database}/_all_docs?include_docs=true");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<AllDocsResponse<T>>(json);

        return result?.Rows.Select(r => r.Doc).ToList() ?? new List<T>();
    }

   

public class AllDocsResponse<T>
{
    [JsonPropertyName("rows")]
    public List<Row<T>> Rows { get; set; } = new();
}

public class Row<T>
{
    [JsonPropertyName("doc")]
    public T Doc { get; set; }
}


    public async Task EnsureAdminExistsAsync()
    {
        var users = await GetAllAsync<User>();

        var adminExists = users.Any(u =>
    string.Equals(u.role, "admin", StringComparison.OrdinalIgnoreCase)
);

        if (!adminExists)
        {
            var passwordHash = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes("admin123"))
            );

            var admin = new User
            {
                _id = "user:admin",
                firstName = "System",
                lastName = "Admin",
                email = "admin@system.com",
                passwordHash = passwordHash,
                role = "admin",
                status = "approved"
            };

            await CreateAsync(admin._id, admin);
        }
    }

    public async Task UpdateAsync<T>(string id, T document)
    {
        var json = JsonSerializer.Serialize(document);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"/{_database}/{id}", content);
        response.EnsureSuccessStatusCode();
    }
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var users = await GetAllAsync<User>();

        return users.FirstOrDefault(u =>
            string.Equals(u.email, email, StringComparison.OrdinalIgnoreCase)
        );
    }

}
