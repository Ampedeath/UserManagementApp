using System.Net;
using FluentAssertions;
using UserManagementApp.Core.DTOs.Users;
using UserManagementApp.Core.Enums;
using UserManagementApp.Core.Models;
using UserManagementApp.Tests.Core;
using UserManagementApp.Tests.Core.Clients;
using static UserManagementApp.Tests.Core.Logging.TestStepLogger;

namespace UserManagementApp.Tests.ApiTests;

public class UsersApiTests : BaseApiTest
{
    [Test]
    public async Task GetUsers()
    {
        User adminUser = null!;
        ApiTestResponse<List<UserResponse>>? response = null;

        await TestCaseStep("Ensure admin test user exists", async () =>
        {
            adminUser = await TestData.EnsureAdminUserExistsAsync();
        });

        await TestCaseStep("Send request to get users as admin", async () =>
        {
            response = await UsersApiClient.GetUsersAsync(adminUser.UserId);
        });

        TestCaseStep("Verify users list response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccessStatusCode.Should().BeTrue();

            response.Body.Should().NotBeNull();
            response.Body.Should().Contain(user => user.UserName == "admin");
        });
    }

    [Test]
    public async Task GetUserById()
    {
        User adminUser = null!;
        User regularUser = null!;
        ApiTestResponse<UserResponse>? response = null;

        await TestCaseStep("Ensure test users exist", async () =>
        {
            adminUser = await TestData.EnsureAdminUserExistsAsync();
            regularUser = await TestData.EnsureRegularUserExistsAsync();
        });

        await TestCaseStep("Send request to get user by id", async () =>
        {
            response = await UsersApiClient.GetUserByIdAsync(regularUser.UserId, adminUser.UserId);
        });

        TestCaseStep("Verify user response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccessStatusCode.Should().BeTrue();

            response.Body.Should().NotBeNull();
            response.Body!.UserId.Should().Be(regularUser.UserId);
            response.Body.UserName.Should().Be("user");
            response.Body.Email.Should().Be("user@test.com");
            response.Body.Role.Should().Be(UserRole.RegularUser);
        });
    }

    [Test]
    public async Task GetUserById_WhenUserDoesNotExist()
    {
        User adminUser = null!;
        ApiTestResponse<UserResponse>? response = null;

        await TestCaseStep("Ensure admin test user exists", async () =>
        {
            adminUser = await TestData.EnsureAdminUserExistsAsync();
        });

        await TestCaseStep("Send request to get non existing user", async () =>
        {
            response = await UsersApiClient.GetUserByIdAsync(999999, adminUser.UserId);
        });

        TestCaseStep("Verify not found response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.IsSuccessStatusCode.Should().BeFalse();
            response.Body.Should().BeNull();
        });
    }

    [Test]
    public async Task CreateUser_WhenAdminCreates()
    {
        User adminUser = null!;
        CreateUserRequest request = null!;
        ApiTestResponse<UserResponse>? response = null;

        var uniqueValue = Guid.NewGuid().ToString("N")[..8];
        var email = $"apiCreated-{uniqueValue}@test.com";

        await TestCaseStep("Ensure admin test user exists", async () =>
        {
            adminUser = await TestData.EnsureAdminUserExistsAsync();
        });

        TestCaseStep("Prepare create user request", () =>
        {
            request = new CreateUserRequest
            {
                UserName = $"apiCreated-{uniqueValue}",
                Email = email,
                Password = "123456",
                Role = UserRole.RegularUser,
                FirstName = "Api",
                LastName = "Created"
            };
        });

        await TestCaseStep("Send request to create user", async () =>
        {
            response = await UsersApiClient.CreateUserAsync(request, adminUser.UserId);
        });

        TestCaseStep("Verify created user response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.Created);
            response.IsSuccessStatusCode.Should().BeTrue();

            response.Body.Should().NotBeNull();
            response.Body!.UserId.Should().BeGreaterThan(0);
            response.Body.UserName.Should().Be(request.UserName);
            response.Body.Email.Should().Be(request.Email);
            response.Body.Role.Should().Be(UserRole.RegularUser);
            response.Body.FirstName.Should().Be("Api");
            response.Body.LastName.Should().Be("Created");
        });
    }

    [Test]
    public async Task UpdateExistingUser()
    {
        User adminUser = null!;
        User targetUser = null!;
        UpdateUserRequest request = null!;
        ApiTestResponse<UserResponse>? response = null;

        var uniqueValue = Guid.NewGuid().ToString("N")[..8];
        var originalEmail = $"apiUpdate-{uniqueValue}@test.com";
        var updatedEmail = $"apiUpdated-{uniqueValue}@test.com";

        await TestCaseStep("Prepare admin and target user", async () =>
        {
            adminUser = await TestData.EnsureAdminUserExistsAsync();

            targetUser = await TestData.CreateTemporaryUserAsync(
                userName: $"apiUpdate-{uniqueValue}",
                email: originalEmail,
                password: "123456",
                role: UserRole.RegularUser,
                firstName: "Before",
                lastName: "Update");
        });

        TestCaseStep("Prepare update user request", () =>
        {
            request = new UpdateUserRequest
            {
                UserName = $"apiUpdated-{uniqueValue}",
                Email = updatedEmail,
                Role = UserRole.Admin,
                FirstName = "After",
                LastName = null
            };
        });

        await TestCaseStep("Send request to update user", async () =>
        {
            response = await UsersApiClient.UpdateUserAsync(
                targetUser.UserId,
                request,
                adminUser.UserId);
        });

        TestCaseStep("Verify updated user response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccessStatusCode.Should().BeTrue();

            response.Body.Should().NotBeNull();
            response.Body!.UserId.Should().Be(targetUser.UserId);
            response.Body.UserName.Should().Be(request.UserName);
            response.Body.Email.Should().Be(request.Email);
            response.Body.Role.Should().Be(UserRole.Admin);
            response.Body.FirstName.Should().Be("After");
            response.Body.LastName.Should().BeNull();
        });
    }

    [Test]
    public async Task UpdateUser_WhenUserDoesNotExist()
    {
        User adminUser = null!;
        UpdateUserRequest request = null!;
        ApiTestResponse<UserResponse>? response = null;

        await TestCaseStep("Ensure admin test user exists", async () =>
        {
            adminUser = await TestData.EnsureAdminUserExistsAsync();
        });

        TestCaseStep("Prepare update request", () =>
        {
            request = new UpdateUserRequest
            {
                UserName = "notExisting-user",
                Email = "notExistingUser@test.com",
                Role = UserRole.RegularUser,
                FirstName = "Not",
                LastName = "Existing"
            };
        });

        await TestCaseStep("Send request to update non existing user", async () =>
        {
            response = await UsersApiClient.UpdateUserAsync(999999, request, adminUser.UserId);
        });

        TestCaseStep("Verify not found response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.IsSuccessStatusCode.Should().BeFalse();
            response.Body.Should().BeNull();
        });
    }

    [Test]
    public async Task DeleteExistingUser_ReturnsNoContent()
    {
        User adminUser = null!;
        User targetUser = null!;
        ApiTestResponse<object>? response = null;

        var uniqueValue = Guid.NewGuid().ToString("N")[..8];
        var email = $"apiDelete-{uniqueValue}@test.com";

        await TestCaseStep("Prepare admin and target user", async () =>
        {
            adminUser = await TestData.EnsureAdminUserExistsAsync();

            targetUser = await TestData.CreateTemporaryUserAsync(
                userName: $"apiDelete-{uniqueValue}",
                email: email,
                password: "123456",
                role: UserRole.RegularUser,
                firstName: "Delete",
                lastName: "Me");
        });

        await TestCaseStep("Send request to delete user", async () =>
        {
            response = await UsersApiClient.DeleteUserAsync(targetUser.UserId, adminUser.UserId);
        });

        TestCaseStep("Verify delete user response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.NoContent);
            response.IsSuccessStatusCode.Should().BeTrue();
        });
    }

    [Test]
    public async Task DeleteUser_WhenUserDoesNotExist()
    {
        User adminUser = null!;
        ApiTestResponse<object>? response = null;

        await TestCaseStep("Ensure admin test user exists", async () =>
        {
            adminUser = await TestData.EnsureAdminUserExistsAsync();
        });

        await TestCaseStep("Send request to delete non existing user", async () =>
        {
            response = await UsersApiClient.DeleteUserAsync(999999, adminUser.UserId);
        });

        TestCaseStep("Verify not found response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.IsSuccessStatusCode.Should().BeFalse();
        });
    }

    [Test]
    public async Task CreateUser_WhenRegularUserTriesToCreateUser_ReturnsForbidden()
    {
        User regularUser = null!;
        CreateUserRequest request = null!;
        ApiTestResponse<UserResponse>? response = null;

        var uniqueValue = Guid.NewGuid().ToString("N")[..8];

        await TestCaseStep("Ensure regular test user exists", async () =>
        {
            regularUser = await TestData.EnsureRegularUserExistsAsync();
        });

        TestCaseStep("Prepare create user request", () =>
        {
            request = new CreateUserRequest
            {
                UserName = $"regularCreated-{uniqueValue}",
                Email = $"regularCreated-{uniqueValue}@test.com",
                Password = "123456",
                Role = UserRole.RegularUser,
                FirstName = "Regular",
                LastName = "Blocked"
            };
        });

        await TestCaseStep("Send create user request as regular user", async () =>
        {
            response = await UsersApiClient.CreateUserAsync(request, regularUser.UserId);
        });

        TestCaseStep("Verify forbidden response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            response.IsSuccessStatusCode.Should().BeFalse();
            response.Body.Should().BeNull();
        });
    }
}