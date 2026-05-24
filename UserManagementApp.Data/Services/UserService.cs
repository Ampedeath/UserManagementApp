using Microsoft.EntityFrameworkCore;
using UserManagementApp.Core.DTOs.Users;
using UserManagementApp.Core.Interfaces;
using UserManagementApp.Core.Models;
using UserManagementApp.Data.Database;

namespace UserManagementApp.Data.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(AppDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserResponse>> GetAllUsersAsync()
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Select(user => new UserResponse
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<UserResponse?> GetUserByIdAsync(int id)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.UserId == id)
            .Select(user => new UserResponse
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = request.Role,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync();

        return MapToUserResponse(user);
    }

    public async Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.UserId == id);

        if (existingUser is null)
        {
            return null;
        }

        existingUser.UserName = request.UserName;
        existingUser.Email = request.Email;
        existingUser.Role = request.Role;
        existingUser.FirstName = request.FirstName;
        existingUser.LastName = request.LastName;
        existingUser.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToUserResponse(existingUser);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.UserId == id);

        if (user is null)
        {
            return false;
        }

        _dbContext.Users.Remove(user);

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}