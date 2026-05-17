using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserManagementApp.Core.DTOs.Users;
using UserManagementApp.Core.Enums;
using UserManagementApp.Core.Models;
using UserManagementApp.Data.Database;
using UserManagementApp.Data.Services;

namespace UserManagementApp.Tests.Unit.Services;

public class UserServiceTests
{
   [Test]
   public async Task CreateUser_WithValidRequest()
   {
      await using var dbContext = CreateDbContext();
      var userService = new UserService(dbContext);

      var request = new CreateUserRequest
      {
         UserName = "testuser",
         Email = "testuser@test.com",
         Password = "123456",
         Role = UserRole.RegularUser,
         FirstName = "Test",
         LastName = "User"
      };
      
      var result = await userService.CreateUserAsync(request);

      result.Should().NotBeNull();
      result.UserId.Should().BeGreaterThan(0);
      result.UserName.Should().Be("testuser");
      result.Email.Should().Be("testuser@test.com");
      result.Role.Should().Be(UserRole.RegularUser);
      result.FirstName.Should().Be("Test");
      result.LastName.Should().Be("User");

      var userFromDb = await dbContext.Users.FirstOrDefaultAsync(user => user.UserId == result.UserId);

      userFromDb.Should().NotBeNull();
      userFromDb.UserName.Should().Be("testuser");
      userFromDb.PasswordHash.Should().Be("123456");
   }
   
   [Test]
    public async Task GetAllUsers_WhenUsersExist_ReturnsUsers()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Users.AddRange(
            new User
            {
                UserName = "admin",
                Email = "admin@test.com",
                PasswordHash = "123456",
                Role = UserRole.Admin,
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserName = "regular",
                Email = "regular@test.com",
                PasswordHash = "123456",
                Role = UserRole.RegularUser,
                FirstName = "Regular",
                LastName = null,
                CreatedAt = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();

        var userService = new UserService(dbContext);

        var result = await userService.GetAllUsersAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(user => user.UserName == "admin");
        result.Should().Contain(user => user.UserName == "regular");
    }

    [Test]
    public async Task GetUserById_WhenUserExists()
    {
        await using var dbContext = CreateDbContext();

        var user = new User
        {
            UserName = "admin",
            Email = "admin@test.com",
            PasswordHash = "123456",
            Role = UserRole.Admin,
            FirstName = "Admin",
            LastName = "User",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var userService = new UserService(dbContext);

        var result = await userService.GetUserByIdAsync(user.UserId);

        result.Should().NotBeNull();
        result.UserId.Should().Be(user.UserId);
        result.UserName.Should().Be("admin");
        result.Role.Should().Be(UserRole.Admin);
    }

    [Test]
    public async Task GetUserById_WhenUserDoesNotExist_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var userService = new UserService(dbContext);

        var result = await userService.GetUserByIdAsync(999);

        result.Should().BeNull();
    }

    [Test]
    public async Task UpdateUser_WhenUserExists_UpdatesUser()
    {
        await using var dbContext = CreateDbContext();

        var user = new User
        {
            UserName = "oldname",
            Email = "old@test.com",
            PasswordHash = "123456",
            Role = UserRole.RegularUser,
            FirstName = "Old",
            LastName = "Name",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var userService = new UserService(dbContext);

        var request = new UpdateUserRequest
        {
            UserName = "newname",
            Email = "new@test.com",
            Role = UserRole.Admin,
            FirstName = "New",
            LastName = null
        };

        var result = await userService.UpdateUserAsync(user.UserId, request);

        result.Should().NotBeNull();
        result.UserName.Should().Be("newname");
        result.Email.Should().Be("new@test.com");
        result.Role.Should().Be(UserRole.Admin);
        result.FirstName.Should().Be("New");
        result.LastName.Should().BeNull();
        result.UpdatedAt.Should().NotBeNull();

        var userFromDb = await dbContext.Users.FirstAsync(dbUser => dbUser.UserId == user.UserId);

        userFromDb.UserName.Should().Be("newname");
        userFromDb.Email.Should().Be("new@test.com");
        userFromDb.Role.Should().Be(UserRole.Admin);
    }

    [Test]
    public async Task UpdateUser_WhenUserDoesNotExist_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var userService = new UserService(dbContext);

        var request = new UpdateUserRequest
        {
            UserName = "newname",
            Email = "new@test.com",
            Role = UserRole.Admin,
            FirstName = "New",
            LastName = null
        };

        var result = await userService.UpdateUserAsync(999, request);

        result.Should().BeNull();
    }

    [Test]
    public async Task DeleteUser_WhenUserExists_DeletesUser()
    {
        await using var dbContext = CreateDbContext();

        var user = new User
        {
            UserName = "delete-me",
            Email = "delete@test.com",
            PasswordHash = "123456",
            Role = UserRole.RegularUser,
            FirstName = "Delete",
            LastName = "Me",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var userService = new UserService(dbContext);

        var result = await userService.DeleteUserAsync(user.UserId);

        result.Should().BeTrue();

        var userFromDb = await dbContext.Users.FirstOrDefaultAsync(dbUser => dbUser.UserId == user.UserId);

        userFromDb.Should().BeNull();
    }

    [Test]
    public async Task DeleteUser_WhenUserDoesNotExist_ReturnsFalse()
    {
        await using var dbContext = CreateDbContext();
        var userService = new UserService(dbContext);

        var result = await userService.DeleteUserAsync(999);

        result.Should().BeFalse();
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}