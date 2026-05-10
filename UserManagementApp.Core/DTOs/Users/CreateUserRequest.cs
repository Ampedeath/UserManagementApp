using UserManagementApp.Core.Enums;

namespace UserManagementApp.Core.DTOs.Users;

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.RegularUser;

    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; }
}