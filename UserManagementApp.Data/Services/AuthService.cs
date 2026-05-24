using Microsoft.EntityFrameworkCore;
using UserManagementApp.Core.DTOs.Auth;
using UserManagementApp.Core.Enums;
using UserManagementApp.Core.Interfaces;
using UserManagementApp.Data.Database;

namespace UserManagementApp.Data.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(AppDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await  _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.UserName == request.UserName);
        
        if (user == null)
            return null;

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        
        if (!isPasswordValid)
            return null;


        return new LoginResponse
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role,
            Message = Convert.ToBoolean(user.Role == UserRole.Admin)
                ? $"Admin {user.UserName} successfully logged in."
                : $"User {user.UserName} successfully logged in."
        };
    }
}