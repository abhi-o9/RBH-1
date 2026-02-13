namespace backend.Models;

public class RegisterRequest
{
    public string firstName { get; set; } = default!;
    public string lastName { get; set; } = default!;
    public string email { get; set; } = default!;
    public string password { get; set; } = default!;
    public string role { get; set; } = "user"; // requested role
}
