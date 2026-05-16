using Microsoft.EntityFrameworkCore;
using UserManagementApp.Core.DTOs.Auth;
using UserManagementApp.Core.Enums;
using UserManagementApp.Core.Interfaces;
using UserManagementApp.Data.Database;

namespace UserManagementApp.Data.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;

    public AuthService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.UserName == request.UserName && user.PasswordHash == request.Password)
            .Select(user => new LoginResponse
            {
                UserId = user.UserId,
                UserName =  user.UserName,
                Email = user.Email,
                Role = user.Role,
                Message = Convert.ToBoolean(user.Role == UserRole.Admin) ? $"Admin {user.UserName} successfully logged in." : $"User {user.UserName} successfully logged in."
            })
            .FirstOrDefaultAsync();
    }
}