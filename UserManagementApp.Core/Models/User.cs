using UserManagementApp.Core.Enums;

namespace UserManagementApp.Core.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.RegularUser;

    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<SupportRequest> SupportRequests { get; set; } = new();
}