using System.Net;
using FluentAssertions;
using UserManagementApp.Core.DTOs.Auth;
using UserManagementApp.Core.Enums;
using UserManagementApp.Tests.Core;
using UserManagementApp.Tests.Core.Clients;
using static UserManagementApp.Tests.Core.Logging.TestStepLogger;

namespace UserManagementApp.Tests.ApiTests;

public class AuthApiTests : BaseApiTest
{
    [Test]
    public async Task LoginAsAdmin_WithValidUserNameAndPassword()
    {
        AuthApiClient authApiClient = null!;
        LoginRequest request = null!;
        ApiTestResponse<LoginResponse>? response = null;

        await TestCaseStep("Ensure admin test user exists", async () =>
        {
            await TestData.EnsureAdminUserExistsAsync();
        });

        TestCaseStep("Create auth API client", () =>
        {
            authApiClient = new AuthApiClient(HttpClient, Log);
        });

        TestCaseStep("Prepare valid login request with username", () =>
        {
            request = new LoginRequest
            {
                UserName = "admin",
                Password = "123456"
            };
        });

        await TestCaseStep("Send login request", async () =>
        {
            response = await authApiClient.LoginAsync(request);
        });

        TestCaseStep("Verify login API response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccessStatusCode.Should().BeTrue();

            response.Body.Should().NotBeNull();
            response.Body!.UserName.Should().Be("admin");
            response.Body.Email.Should().Be("admin@test.com");
            response.Body.Role.Should().Be(UserRole.Admin);
        });
    }
    
    [Test]
    public async Task LoginAsRegular_WithValidUserNameAndPassword()
    {
        AuthApiClient authApiClient = null!;
        LoginRequest request = null!;
        ApiTestResponse<LoginResponse>? response = null;

        await TestCaseStep("Ensure regular test user exists", async () =>
        {
            await TestData.EnsureRegularUserExistsAsync();
        });

        TestCaseStep("Create auth API client", () =>
        {
            authApiClient = new AuthApiClient(HttpClient, Log);
        });

        TestCaseStep("Prepare valid login request with regular username", () =>
        {
            request = new LoginRequest
            {
                UserName = "user",
                Password = "123456"
            };
        });

        await TestCaseStep("Send login request", async () =>
        {
            response = await authApiClient.LoginAsync(request);
        });

        TestCaseStep("Verify regular user login response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccessStatusCode.Should().BeTrue();

            response.Body.Should().NotBeNull();
            response.Body!.UserName.Should().Be("user");
            response.Body.Email.Should().Be("user@test.com");
            response.Body.Role.Should().Be(UserRole.RegularUser);
        });
    }

    [Test]
    public async Task LoginWithInvalidPassword_ReturnsUnauthorized()
    {
        AuthApiClient authApiClient = null!;
        LoginRequest request = null!;
        ApiTestResponse<LoginResponse>? response = null;

        await TestCaseStep("Ensure admin test user exists", async () =>
        {
            await TestData.EnsureAdminUserExistsAsync();
        });

        TestCaseStep("Create auth API client", () =>
        {
            authApiClient = new AuthApiClient(HttpClient, Log);
        });

        TestCaseStep("Prepare login request with invalid password", () =>
        {
            request = new LoginRequest
            {
                UserName = "admin",
                Password = "wrong-password"
            };
        });

        await TestCaseStep("Send login request", async () =>
        {
            response = await authApiClient.LoginAsync(request);
        });

        TestCaseStep("Verify unauthorized response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.IsSuccessStatusCode.Should().BeFalse();
            response.Body.Should().BeNull();

            response.RawBody.Should().NotBeNullOrWhiteSpace();
            response.RawBody.Should().Contain("Invalid");
        });
    }

    [Test]
    public async Task LoginWhenUserDoesNotExist_ReturnsUnauthorized()
    {
        AuthApiClient authApiClient = null!;
        LoginRequest request = null!;
        ApiTestResponse<LoginResponse>? response = null;

        TestCaseStep("Create auth API client", () =>
        {
            authApiClient = new AuthApiClient(HttpClient, Log);
        });

        TestCaseStep("Prepare login request for non-existing user", () =>
        {
            request = new LoginRequest
            {
                UserName = "unknown-user",
                Password = "123456"
            };
        });

        await TestCaseStep("Send login request", async () =>
        {
            response = await authApiClient.LoginAsync(request);
        });

        TestCaseStep("Verify unauthorized response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.IsSuccessStatusCode.Should().BeFalse();
            response.Body.Should().BeNull();

            response.RawBody.Should().NotBeNullOrWhiteSpace();
            response.RawBody.Should().Contain("Invalid");
        });
    }

    [Test]
    public async Task LoginWhenUserNameIsEmpty_ReturnsUnauthorized()
    {
        AuthApiClient authApiClient = null!;
        LoginRequest request = null!;
        ApiTestResponse<LoginResponse>? response = null;

        TestCaseStep("Create auth API client", () =>
        {
            authApiClient = new AuthApiClient(HttpClient, Log);
        });

        TestCaseStep("Prepare login request with empty username", () =>
        {
            request = new LoginRequest
            {
                UserName = "",
                Password = "123456"
            };
        });

        await TestCaseStep("Send login request", async () =>
        {
            response = await authApiClient.LoginAsync(request);
        });

        TestCaseStep("Verify unauthorized response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.IsSuccessStatusCode.Should().BeFalse();
            response.Body.Should().BeNull();
        });
    }

    [Test]
    public async Task LoginWhenPasswordIsEmpty_ReturnsUnauthorized()
    {
        AuthApiClient authApiClient = null!;
        LoginRequest request = null!;
        ApiTestResponse<LoginResponse>? response = null;

        await TestCaseStep("Ensure admin test user exists", async () =>
        {
            await TestData.EnsureAdminUserExistsAsync();
        });

        TestCaseStep("Create auth API client", () =>
        {
            authApiClient = new AuthApiClient(HttpClient, Log);
        });

        TestCaseStep("Prepare login request with empty password", () =>
        {
            request = new LoginRequest
            {
                UserName = "admin",
                Password = ""
            };
        });

        await TestCaseStep("Send login request", async () =>
        {
            response = await authApiClient.LoginAsync(request);
        });

        TestCaseStep("Verify unauthorized response", () =>
        {
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.IsSuccessStatusCode.Should().BeFalse();
            response.Body.Should().BeNull();
        });
    }
}