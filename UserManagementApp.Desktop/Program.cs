using Microsoft.Extensions.Configuration;
using UserManagementApp.Core.DTOs.Auth;
using UserManagementApp.Core.DTOs.Users;
using UserManagementApp.Core.Enums;
using UserManagementApp.Desktop.Clients;
using UserManagementApp.Desktop.Configuration;

Console.WriteLine("User Management Desktop Client");
Console.WriteLine("--------------------------------");

var configuration = ConfigurationFactory.CreateConfiguration();

var apiSettings = configuration
    .GetSection("ApiSettings")
    .Get<ApiSettings>();

if (apiSettings is null || string.IsNullOrWhiteSpace(apiSettings.BaseUrl))
{
    Console.WriteLine("API base URL is not configured.");
    return;
}

using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri(apiSettings.BaseUrl);

var authApiClient = new AuthApiClient(httpClient);
var userApiClient = new UserApiClient(httpClient);

LoginResponse? currentUser = null;

while (true)
{
    Console.Clear();

    PrintHeader(currentUser);

    if (currentUser is null)
    {
        Console.WriteLine("1. Login");
        Console.WriteLine("0. Exit");
        Console.WriteLine();
        
        Console.WriteLine("Choose an option:");
        var option = Console.ReadLine();

        switch (option)
        {
            case "1":
                currentUser = await LoginAsync(authApiClient);
                break;
            case "0":
                return;
            default:
                PrintError("Invalid option.");
                return;
        }
        
        Continue();
        continue;
    }
    
    Console.WriteLine("1. Show users");
    
    if (currentUser.Role == UserRole.Admin)
    {
        Console.WriteLine("2. Create user");
        Console.WriteLine("3. Update user");
        Console.WriteLine("4. Delete user");
    }

    Console.WriteLine("5. Logout");
    Console.WriteLine("0. Exit");
    Console.WriteLine();

    Console.Write("Choose option: ");
    var selectedOption = Console.ReadLine();

    switch (selectedOption)
    {
        case "1":
            await ShowUsersAsync(userApiClient, currentUser);
            break;
        case "2" when currentUser.Role == UserRole.Admin:
            await CreateUserAsync(userApiClient, currentUser);
            break;
        case "3" when currentUser.Role == UserRole.Admin:
            await UpdateUserAsync(userApiClient, currentUser);
            break;
        case "4" when currentUser.Role == UserRole.Admin:
            await DeleteUserAsync(userApiClient, currentUser);
            break;
        case "5":
            currentUser = null;
            Console.WriteLine("Logged out.");
            break;
        case "0":
            return;
        default:
            PrintError("Invalid option or permission denied.");
            break;
    }

    Continue();
}

static void PrintHeader(LoginResponse? currentUser)
{
    Console.WriteLine("User Management Desktop Client");
    Console.WriteLine("--------------------------------");

    Console.WriteLine(currentUser is null
        ? "Status: Not logged in"
        : $"Logged in as: {currentUser.UserName} | Role: {currentUser.Role}");

    Console.WriteLine();
}

static async Task<LoginResponse?> LoginAsync(AuthApiClient authApiClient)
{
    Console.WriteLine("Username: ");
    var userName = Console.ReadLine() ?? string.Empty;
    
    Console.WriteLine("Password: ");
    var pass = Console.ReadLine() ?? string.Empty;

    try
    {
        var loginResult = await authApiClient.LoginAsync(userName, pass);
        if (loginResult is null)
        {
            PrintError("Login failed. Invalid username/email or password.");
            return null;
        }
        
        Console.WriteLine();
        Console.WriteLine($"Login successful. Hello, {loginResult.UserName}!");
        
        return loginResult;
    }
    catch (HttpRequestException  ex)
    {
        PrintError("API request failed. \n" + ex.Message);
        return null;
    }
}

static async Task ShowUsersAsync(UserApiClient userApiClient, LoginResponse currentUser)
{
    try
    {
        var users = await userApiClient.GetUsersAsync(currentUser.UserId);

        if (users.Count is 0)
        {
            Console.WriteLine("No users found.");
            return;
        }
        
        Console.WriteLine("Users: ");
        Console.WriteLine();
        
        foreach (var user in users)
            Console.WriteLine($"{user.UserId}. {user.UserName} | {user.Email} | {user.Role}");
    }
    catch (HttpRequestException ex)
    {
        PrintError("Could not load users.\n" + ex.Message);
    }
}

static async Task CreateUserAsync(UserApiClient userApiClient, LoginResponse currentUser)
{
    var request = new CreateUserRequest();
    Console.Write("Username: ");
    request.UserName = Console.ReadLine() ?? string.Empty;

    Console.Write("Email: ");
    request.Email = Console.ReadLine() ?? string.Empty;

    Console.Write("Password: ");
    request.Password = Console.ReadLine() ?? string.Empty;

    request.Role = ReadUserRole();

    Console.Write("First name: ");
    request.FirstName = Console.ReadLine() ?? string.Empty;

    Console.Write("Last name optional: ");
    request.LastName = ReadOptionalString();

    try
    {
        var createdUser = await userApiClient.CreateUserAsync(request, currentUser.UserId);
        Console.WriteLine();
        Console.WriteLine($"User created successfully: {createdUser.UserId} | {createdUser.UserName}");
    }
    catch (HttpRequestException ex)
    {
        PrintError("Could not create user.\n" + ex.Message);
    }
}

static async Task UpdateUserAsync(UserApiClient userApiClient, LoginResponse currentUser)
{
    var userId = ReadInt("User id to update: ");

    var existingUser = await userApiClient.GetUserByIdAsync(userId, currentUser.UserId);

    if (existingUser is null)
    {
        PrintError($"User with id {userId} was not found.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Current user: {existingUser.UserName} | {existingUser.Email} | {existingUser.Role}");
    Console.WriteLine("Enter new values.");
    Console.WriteLine();

    var request = new UpdateUserRequest();

    Console.Write("Username: ");
    request.UserName = Console.ReadLine() ?? string.Empty;

    Console.Write("Email: ");
    request.Email = Console.ReadLine() ?? string.Empty;

    request.Role = ReadUserRole();

    Console.Write("First name: ");
    request.FirstName = Console.ReadLine() ?? string.Empty;

    Console.Write("Last name optional: ");
    request.LastName = ReadOptionalString();

    try
    {
        var updatedUser = await userApiClient.UpdateUserAsync(
            userId,
            request,
            currentUser.UserId);

        if (updatedUser is null)
        {
            PrintError($"User with id {userId} was not found.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"User updated successfully: {updatedUser.UserId} | {updatedUser.UserName}");
    }
    catch (HttpRequestException exception)
    {
        PrintError("Could not update user.");
        Console.WriteLine(exception.Message);
    }
}

static async Task DeleteUserAsync(UserApiClient userApiClient, LoginResponse currentUser)
{
    var userId = ReadInt("User id to delete: ");

    if (userId == currentUser.UserId)
    {
        PrintError("You cannot delete your own account while you are logged in.");
        return;
    }

    try
    {
        var isDeleted = await userApiClient.DeleteUserAsync(userId, currentUser.UserId);

        if (!isDeleted)
        {
            PrintError($"User with id {userId} was not found.");
            return;
        }

        Console.WriteLine("User deleted successfully.");
    }
    catch (HttpRequestException exception)
    {
        PrintError("Could not delete user.");
        Console.WriteLine(exception.Message);
    }
}

static UserRole ReadUserRole()
{
    while (true)
    {
        Console.WriteLine("Choose role:");
        Console.WriteLine("1. RegularUser");
        Console.WriteLine("2. Admin");
        Console.Write("Role: ");

        var input = Console.ReadLine();

        switch (input)
        {
            case "1":
                return UserRole.RegularUser;

            case "2":
                return UserRole.Admin;

            default:
                PrintError("Invalid role. Please choose 1 or 2.");
                break;
        }
    }
}

static int ReadInt(string message)
{
    while (true)
    {
        Console.Write(message);

        var input = Console.ReadLine();

        if (int.TryParse(input, out var value))
        {
            return value;
        }

        PrintError("Invalid number. Please try again.");
    }
}

static string? ReadOptionalString()
{
    var value = Console.ReadLine();

    return string.IsNullOrWhiteSpace(value)
        ? null
        : value;
}

static void PrintError(string message)
{
    Console.WriteLine();
    Console.WriteLine($"Error: {message}");
}

static void Continue()
{
    Console.WriteLine();
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}
