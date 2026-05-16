using UserManagementApp.Core.Enums;

namespace UserManagementApp.Core.DTOs.Auth;

public class LoginResponse
{
    public int UserId { get; set; }
    
    public string UserName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } 
    
    public string Message { get; set; } = string.Empty;
}