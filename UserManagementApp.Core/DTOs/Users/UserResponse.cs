using UserManagementApp.Core.Enums;

namespace UserManagementApp.Core.DTOs.Users;

public class UserResponse
{
    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}