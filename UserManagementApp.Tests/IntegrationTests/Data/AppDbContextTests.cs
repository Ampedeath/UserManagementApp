using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UserManagementApp.Core.Enums;
using UserManagementApp.Core.Models;
using UserManagementApp.Data.Database;
using static UserManagementApp.Tests.Core.Logging.TestStepLogger;

namespace UserManagementApp.Tests.IntegrationTests.Data;

public class AppDbContextTests
{
    private SqliteConnection _connection = null!;
    private AppDbContext _dbContext = null!;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

        _dbContext = new AppDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task SaveChanges_WhenUserIsValid()
    {
        User user = null!;

        TestCaseStep("Prepare valid user", () =>
        {
            user = CreateUser(
                userName: "validuser",
                email: "valid@test.com",
                firstName: "Valid");
        });

        await TestCaseStep("Save user to database", async () =>
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        });

        await TestCaseStep("Verify user was saved", async () =>
        {
            var savedUser = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Email == "valid@test.com");

            savedUser.Should().NotBeNull();
            savedUser!.UserName.Should().Be("validuser");
            savedUser.Email.Should().Be("valid@test.com");
            savedUser.FirstName.Should().Be("Valid");
            savedUser.Role.Should().Be(UserRole.RegularUser);
        });
    }

    [Test]
    public async Task SaveChanges_WhenEmailIsDuplicated()
    {
        var firstUser = CreateUser("user1", "duplicate@test.com", "User");
        var secondUser = CreateUser("user2", "duplicate@test.com", "User");

        await TestCaseStep("Save first user", async () =>
        {
            _dbContext.Users.Add(firstUser);
            await _dbContext.SaveChangesAsync();
        });

        await TestCaseStep("Try to save second user with duplicate email", async () =>
        {
            _dbContext.Users.Add(secondUser);

            var action = async () => await _dbContext.SaveChangesAsync();

            await action.Should().ThrowAsync<DbUpdateException>();
        });
    }

    [Test]
    public async Task SaveChanges_WhenUserNameIsDuplicated()
    {
        var firstUser = CreateUser("duplicate", "user1@test.com", "User");
        var secondUser = CreateUser("duplicate", "user2@test.com", "User");

        await TestCaseStep("Save first user", async () =>
        {
            _dbContext.Users.Add(firstUser);
            await _dbContext.SaveChangesAsync();
        });

        await TestCaseStep("Try to save second user with duplicate username", async () =>
        {
            _dbContext.Users.Add(secondUser);

            var action = async () => await _dbContext.SaveChangesAsync();

            await action.Should().ThrowAsync<DbUpdateException>();
        });
    }

    [Test]
    public async Task SaveChanges_WhenFirstNameIsMissing()
    {
        var user = CreateUser("nofirstname", "nofirstname@test.com", firstName: null!);

        await TestCaseStep("Try to save user without first name", async () =>
        {
            _dbContext.Users.Add(user);

            var action = async () => await _dbContext.SaveChangesAsync();

            await action.Should().ThrowAsync<DbUpdateException>();
        });
    }

    private static User CreateUser(string userName, string email, string firstName)
    {
        return new User
        {
            UserName = userName,
            Email = email,
            PasswordHash = "hashed-password",
            Role = UserRole.RegularUser,
            FirstName = firstName,
            LastName = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }
}