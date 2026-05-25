using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserManagementApp.Core.DTOs.Users;
using UserManagementApp.Core.Enums;
using UserManagementApp.Core.Models;
using UserManagementApp.Data.Database;
using UserManagementApp.Data.Services;
using static UserManagementApp.Tests.Core.Logging.TestStepLogger;

namespace UserManagementApp.Tests.UnitTests.Services;

public class UserServiceTests
{ 
    private readonly PasswordHasher _passwordHasher = new();

    [Test]
    public async Task CreateUser_WithValidRequest()
    {
        AppDbContext dbContext = null!;
        UserService userService = null!;
        CreateUserRequest request = null!;
        UserResponse? result = null;
        User? userFromDb = null;

        TestCaseStep("Create database context and user service", () =>
        {
            dbContext = CreateDbContext();
            userService = new UserService(dbContext, _passwordHasher);
        });

        TestCaseStep("Prepare create user request", () =>
        {
            request = new CreateUserRequest
            {
                UserName = "testuser",
                Email = "testuser@test.com",
                Password = "123456",
                Role = UserRole.RegularUser,
                FirstName = "Test",
                LastName = "User"
            };
        });

        await TestCaseStep("Create user", async () =>
        {
            result = await userService.CreateUserAsync(request);
        });

        TestCaseStep("Verify created user response", () =>
        {
            result.Should().NotBeNull();
            result!.UserId.Should().BeGreaterThan(0);
            result.UserName.Should().Be("testuser");
            result.Email.Should().Be("testuser@test.com");
            result.Role.Should().Be(UserRole.RegularUser);
            result.FirstName.Should().Be("Test");
            result.LastName.Should().Be("User");
        });

        await TestCaseStep("Get created user from database", async () =>
        {
            userFromDb = await dbContext.Users
                .FirstOrDefaultAsync(user => user.UserId == result!.UserId);
        });

        TestCaseStep("Verify user was saved with hashed password", () =>
        {
            userFromDb.Should().NotBeNull();
            userFromDb!.UserName.Should().Be("testuser");

            userFromDb.PasswordHash.Should().NotBeNullOrWhiteSpace();
            userFromDb.PasswordHash.Should().NotBe("123456");

            _passwordHasher
                .VerifyPassword("123456", userFromDb.PasswordHash)
                .Should()
                .BeTrue();
        });

        await dbContext.DisposeAsync();
    }

    [Test]
    public async Task GetAllUsers_WhenUsersExist()
    {
        AppDbContext dbContext = null!;
        UserService userService = null!;
        List<UserResponse>? result = null;

        await TestCaseStep("Prepare users in database", async () =>
        {
            dbContext = CreateDbContext();

            dbContext.Users.AddRange(
                new User
                {
                    UserName = "admin",
                    Email = "admin@test.com",
                    PasswordHash = _passwordHasher.HashPassword("123456"),
                    Role = UserRole.Admin,
                    FirstName = "Admin",
                    LastName = "User",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserName = "regular",
                    Email = "regular@test.com",
                    PasswordHash = _passwordHasher.HashPassword("123456"),
                    Role = UserRole.RegularUser,
                    FirstName = "Regular",
                    LastName = null,
                    CreatedAt = DateTime.UtcNow
                });

            await dbContext.SaveChangesAsync();

            userService = new UserService(dbContext, _passwordHasher);
        });

        await TestCaseStep("Get all users", async () =>
        {
            result = await userService.GetAllUsersAsync();
        });

        TestCaseStep("Verify users list", () =>
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(user => user.UserName == "admin");
            result.Should().Contain(user => user.UserName == "regular");
        });

        await dbContext.DisposeAsync();
    }

    [Test]
    public async Task GetUserById()
    {
        AppDbContext dbContext = null!;
        UserService userService = null!;
        User user = null!;
        UserResponse? result = null;

        await TestCaseStep("Prepare user in database", async () =>
        {
            dbContext = CreateDbContext();

            user = new User
            {
                UserName = "admin",
                Email = "admin@test.com",
                PasswordHash = _passwordHasher.HashPassword("123456"),
                Role = UserRole.Admin,
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            userService = new UserService(dbContext, _passwordHasher);
        });

        await TestCaseStep("Get user by id", async () =>
        {
            result = await userService.GetUserByIdAsync(user.UserId);
        });

        TestCaseStep("Verify user response", () =>
        {
            result.Should().NotBeNull();
            result!.UserId.Should().Be(user.UserId);
            result.UserName.Should().Be("admin");
            result.Role.Should().Be(UserRole.Admin);
        });

        await dbContext.DisposeAsync();
    }

    [Test]
    public async Task GetUserById_WhenUserDoesNotExist()
    {
        AppDbContext dbContext = null!;
        UserService userService = null!;
        UserResponse? result = null;

        TestCaseStep("Create database context and user service", () =>
        {
            dbContext = CreateDbContext();
            userService = new UserService(dbContext, _passwordHasher);
        });

        await TestCaseStep("Get non existing user by id", async () =>
        {
            result = await userService.GetUserByIdAsync(999);
        });

        TestCaseStep("Verify result is null", () =>
        {
            result.Should().BeNull();
        });

        await dbContext.DisposeAsync();
    }

    [Test]
    public async Task UpdateUser_WhenUserExists()
    {
        AppDbContext dbContext = null!;
        UserService userService = null!;
        User user = null!;
        UpdateUserRequest request = null!;
        UserResponse? result = null;
        User? userFromDb = null;

        await TestCaseStep("Prepare user in database", async () =>
        {
            dbContext = CreateDbContext();

            user = new User
            {
                UserName = "oldname",
                Email = "old@test.com",
                PasswordHash = _passwordHasher.HashPassword("123456"),
                Role = UserRole.RegularUser,
                FirstName = "Old",
                LastName = "Name",
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            userService = new UserService(dbContext, _passwordHasher);
        });

        TestCaseStep("Prepare update user request", () =>
        {
            request = new UpdateUserRequest
            {
                UserName = "newname",
                Email = "new@test.com",
                Role = UserRole.Admin,
                FirstName = "New",
                LastName = null
            };
        });

        await TestCaseStep("Update user", async () =>
        {
            result = await userService.UpdateUserAsync(user.UserId, request);
        });

        TestCaseStep("Verify updated user response", () =>
        {
            result.Should().NotBeNull();
            result!.UserName.Should().Be("newname");
            result.Email.Should().Be("new@test.com");
            result.Role.Should().Be(UserRole.Admin);
            result.FirstName.Should().Be("New");
            result.LastName.Should().BeNull();
            result.UpdatedAt.Should().NotBeNull();
        });

        await TestCaseStep("Get updated user from database", async () =>
        {
            userFromDb = await dbContext.Users
                .FirstAsync(dbUser => dbUser.UserId == user.UserId);
        });

        TestCaseStep("Verify user was updated in database", () =>
        {
            userFromDb!.UserName.Should().Be("newname");
            userFromDb.Email.Should().Be("new@test.com");
            userFromDb.Role.Should().Be(UserRole.Admin);

            userFromDb.PasswordHash.Should().NotBe("123456");
            _passwordHasher.VerifyPassword("123456", userFromDb.PasswordHash).Should().BeTrue();
        });

        await dbContext.DisposeAsync();
    }

    [Test]
    public async Task UpdateUser_WhenUserDoesNotExist()
    {
        AppDbContext dbContext = null!;
        UserService userService = null!;
        UpdateUserRequest request = null!;
        UserResponse? result = null;

        TestCaseStep("Create database context and user service", () =>
        {
            dbContext = CreateDbContext();
            userService = new UserService(dbContext, _passwordHasher);
        });

        TestCaseStep("Prepare update user request", () =>
        {
            request = new UpdateUserRequest
            {
                UserName = "newname",
                Email = "new@test.com",
                Role = UserRole.Admin,
                FirstName = "New",
                LastName = null
            };
        });

        await TestCaseStep("Update non existing user", async () =>
        {
            result = await userService.UpdateUserAsync(999, request);
        });

        TestCaseStep("Verify result is null", () =>
        {
            result.Should().BeNull();
        });

        await dbContext.DisposeAsync();
    }

    [Test]
    public async Task DeleteUser_WhenUserExists()
    {
        AppDbContext dbContext = null!;
        UserService userService = null!;
        User user = null!;
        bool result = false;
        User? userFromDb = null;

        await TestCaseStep("Prepare user in database", async () =>
        {
            dbContext = CreateDbContext();

            user = new User
            {
                UserName = "delete-me",
                Email = "delete@test.com",
                PasswordHash = _passwordHasher.HashPassword("123456"),
                Role = UserRole.RegularUser,
                FirstName = "Delete",
                LastName = "Me",
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            userService = new UserService(dbContext, _passwordHasher);
        });

        await TestCaseStep("Delete user", async () =>
        {
            result = await userService.DeleteUserAsync(user.UserId);
        });

        TestCaseStep("Verify delete result", () =>
        {
            result.Should().BeTrue();
        });

        await TestCaseStep("Get deleted user from database", async () =>
        {
            userFromDb = await dbContext.Users
                .FirstOrDefaultAsync(dbUser => dbUser.UserId == user.UserId);
        });

        TestCaseStep("Verify user was deleted from database", () =>
        {
            userFromDb.Should().BeNull();
        });

        await dbContext.DisposeAsync();
    }

    [Test]
    public async Task DeleteUser_WhenUserDoesNotExist()
    {
        AppDbContext dbContext = null!;
        UserService userService = null!;
        bool result = true;

        TestCaseStep("Create database context and user service", () =>
        {
            dbContext = CreateDbContext();
            userService = new UserService(dbContext, _passwordHasher);
        });

        await TestCaseStep("Delete non existing user", async () =>
        {
            result = await userService.DeleteUserAsync(999);
        });

        TestCaseStep("Verify delete result", () =>
        {
            result.Should().BeFalse();
        });

        await dbContext.DisposeAsync();
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}