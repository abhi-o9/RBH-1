using System.Text.Json.Serialization;

namespace backend.Models;

public class User
{
    [JsonPropertyName("_id")]
    public string _id { get; set; } = null!;

    [JsonPropertyName("_rev")]
    public string? _rev { get; set; }


    public string firstName { get; set; } = null!;

    public string lastName { get; set; } = null!;

    public string email { get; set; } = null!;

    public string passwordHash { get; set; } = null!;

    public string role { get; set; } = null!;

    public string? currentSessionId { get; set; }


    public string status { get; set; } = null!;  // "pending" or "approved"
}
