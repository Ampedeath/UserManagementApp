using Microsoft.EntityFrameworkCore;
using UserManagementApp.Core.Enums;
using UserManagementApp.Core.Models;
using UserManagementApp.Data.Database;
using UserManagementApp.Data.Services;
using UserManagementApp.Tests.Core.Configuration;
using UserManagementApp.Tests.Core.Logging;

namespace UserManagementApp.Tests.Core.Helpers;

public class TestDataHelper
{
    private readonly Action<int>? _registerUserForCleanup;
    private readonly TestLog _log;
    private readonly PasswordHasher _passwordHasher;

    public TestDataHelper(TestLog log, Action<int>? registerUserForCleanup = null)
    {
        _log = log;
        _registerUserForCleanup = registerUserForCleanup;
        _passwordHasher = new PasswordHasher();
    }

    public async Task<User> CreateTemporaryUserAsync(string userName, string email, string password,
        UserRole role,
        string firstName,
        string? lastName)
    {
        var user = await EnsureUserExistsAsync(
            userName,
            email,
            password,
            role,
            firstName,
            lastName);

        _registerUserForCleanup?.Invoke(user.UserId);

        return user;
    }
    
    public async Task<User> EnsureAdminUserExistsAsync()
    {
        return await EnsureUserExistsAsync(
            userName: "admin",
            email: "admin@test.com",
            password: "123456",
            role: UserRole.Admin,
            firstName: "Admin",
            lastName: "User");
    }

    public async Task<User> EnsureRegularUserExistsAsync()
    {
        return await EnsureUserExistsAsync(
            userName: "user",
            email: "user@test.com",
            password: "123456",
            role: UserRole.RegularUser,
            firstName: "Regular",
            lastName: "User");
    }

    public async Task<User> EnsureUserExistsAsync( string userName, string email, string password,
        UserRole role, string firstName, string? lastName)
    {
        await using var dbContext = CreateDbContext();

        var existingUser = await dbContext.Users
            .FirstOrDefaultAsync(user =>
                user.UserName == userName &&
                user.Email == email);

        if (existingUser is not null)
        {
            _log.Info($"Test user already exists: #{existingUser.UserId} {existingUser.UserName} | {existingUser.Email}");

            var passwordIsValid = _passwordHasher.VerifyPassword(password, existingUser.PasswordHash);

            if (!passwordIsValid)
            {
                existingUser.PasswordHash = _passwordHasher.HashPassword(password);
                existingUser.UpdatedAt = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();

                _log.Info($"Password was updated for test user: {existingUser.UserName}");
            }

            return existingUser;
        }

        var user = new User
        {
            UserName = userName,
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password),
            Role = role,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        _log.Info($"Test user was created: #{user.UserId} {user.UserName} | {user.Email} | {user.Role}");

        return user;
    }
    
    private static AppDbContext CreateDbContext()
    {
        var connectionString = TestConfiguration.BuildDatabaseConnectionString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}