using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserManagementApp.Core.DTOs.Auth;
using UserManagementApp.Core.Enums;
using UserManagementApp.Core.Models;
using UserManagementApp.Data.Database;
using UserManagementApp.Data.Services;
using static UserManagementApp.Tests.Core.Logging.TestStepLogger;

namespace UserManagementApp.Tests.UnitTests.Services;

public class AuthServiceTests
{
    private readonly PasswordHasher _passwordHasher = new();

    [Test]
    public async Task LoginAsAdmin_WithValidUserNameAndPassword()
    {
        await using var dbContext = CreateDbContext();

        AuthService authService = null!;
        LoginRequest request = null!;
        LoginResponse? result = null;

        await TestCaseStep("Prepare user in database", async () =>
        {
            dbContext.Users.Add(new User
            {
                UserName = "admin",
                Email = "admin@test.com",
                PasswordHash = _passwordHasher.HashPassword("123456"),
                Role = UserRole.Admin,
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            authService = new AuthService(dbContext, _passwordHasher);
        });

        TestCaseStep("Prepare login request with username", () =>
        {
            request = new LoginRequest
            {
                UserName = "admin",
                Password = "123456"
            };
        });

        await TestCaseStep("Login with valid username and password", async () =>
        {
            result = await authService.LoginAsync(request);
        });

        TestCaseStep("Verify login response", () =>
        {
            result.Should().NotBeNull();
            result.UserName.Should().Be("admin");
            result.Email.Should().Be("admin@test.com");
            result.Role.Should().Be(UserRole.Admin);
        });
    }
    
    [Test]
    public async Task LoginAsRegularUser_WithValidUserNameAndPassword()
    {
        await using var dbContext = CreateDbContext();

        AuthService authService = null!;
        LoginRequest request = null!;
        LoginResponse? result = null;

        await TestCaseStep("Prepare user in database", async () =>
        {
            dbContext.Users.Add(new User
            {
                UserName = "RegularUser",
                Email = "user@test.com",
                PasswordHash = _passwordHasher.HashPassword("123456"),
                Role = UserRole.RegularUser,
                FirstName = "RegularUser",
                LastName = "User",
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            authService = new AuthService(dbContext, _passwordHasher);
        });

        TestCaseStep("Prepare login request with username", () =>
        {
            request = new LoginRequest
            {
                UserName = "RegularUser",
                Password = "123456"
            };
        });

        await TestCaseStep("Login with valid username and password", async () =>
        {
            result = await authService.LoginAsync(request);
        });

        TestCaseStep("Verify login response", () =>
        {
            result.Should().NotBeNull();
            result.UserName.Should().Be("RegularUser");
            result.Email.Should().Be("user@test.com");
            result.Role.Should().Be(UserRole.RegularUser);
        });
    }

    [Test]
    public async Task Login_WithInvalidPassword()
    {
        await using var dbContext = CreateDbContext();

        AuthService authService = null!;
        LoginRequest request = null!;
        LoginResponse? result = null;

        await TestCaseStep("Prepare user in database", async () =>
        {
            dbContext.Users.Add(new User
            {
                UserName = "admin",
                Email = "admin@test.com",
                PasswordHash = _passwordHasher.HashPassword("123456"),
                Role = UserRole.Admin,
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            authService = new AuthService(dbContext, _passwordHasher);
        });

        TestCaseStep("Prepare login request with invalid password", () =>
        {
            request = new LoginRequest
            {
                UserName = "admin",
                Password = "wrong-password"
            };
        });

        await TestCaseStep("Login with invalid password", async () =>
        {
            result = await authService.LoginAsync(request);
        });

        TestCaseStep("Verify login result is null", () =>
        {
            result.Should().BeNull();
        });
    }

    [Test]
    public async Task Login_WhenUserDoesNotExist()
    {
        await using var dbContext = CreateDbContext();

        AuthService authService = null!;
        LoginRequest request = null!;
        LoginResponse? result = null;

        TestCaseStep("Create auth service", () =>
        {
            authService = new AuthService(dbContext, _passwordHasher);
        });

        TestCaseStep("Prepare login request for non-existing user", () =>
        {
            request = new LoginRequest
            {
                UserName = "unknown-user",
                Password = "123456"
            };
        });

        await TestCaseStep("Login with non-existing user", async () =>
        {
            result = await authService.LoginAsync(request);
        });

        TestCaseStep("Verify login result is null", () =>
        {
            result.Should().BeNull();
        });
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}