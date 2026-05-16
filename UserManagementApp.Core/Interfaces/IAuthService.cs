using UserManagementApp.Core.DTOs.Auth;

namespace UserManagementApp.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}