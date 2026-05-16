using Microsoft.AspNetCore.Mvc;
using UserManagementApp.Core.DTOs.Users;
using UserManagementApp.Core.Interfaces;

namespace UserManagementApp.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private IPermissionService  _permissionService;

    public UsersController(IUserService userService, IPermissionService permissionService)
    {
        _userService = userService;
        _permissionService =  permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user is null)
        {
            return NotFound(new
            {
                Message = $"User with id '{id}' was not found."
            });
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser(CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);

        return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, user);
    }

    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(int id, UpdateUserRequest request, [FromHeader(Name = "X-User-Id")] int currentUserId)
    {
        if (currentUserId <= 0)
        {
            return Unauthorized(new
            {
                Message = "Missing or invalid X-User-Id header."
            });
        }
        
        var isAdmin = await _permissionService.IsAdminAsync(currentUserId);
        
        if (!isAdmin)
        {
            return StatusCode(403, new
            {
                Message = "You do not have permission to update users."
            });
        }
        
        var user = await _userService.UpdateUserAsync(id, request);

        if (user is null)
        {
            return NotFound(new
            {
                Message = $"User with id '{id}' was not found."
            });
        }

        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, [FromHeader(Name = "X-User-Id")] int currentUserId)
    {
        if (currentUserId <= 0)
        {
            return Unauthorized(new
            {
                Message = "Missing or invalid X-User-Id header."
            });
        }
        
        var isAdmin = await _permissionService.IsAdminAsync(currentUserId);
        
        if (!isAdmin)
        {
            return StatusCode(403, new
            {
                Message = "You do not have permission to delete users."
            });
        }
        
        var isDeleted = await _userService.DeleteUserAsync(id);

        if (!isDeleted)
        {
            return NotFound(new
            {
                Message = $"User with id '{id}'was not found."
            });
        }

        return NoContent();
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;
        
        if (!Request.Headers.TryGetValue("X-User-Id", out var value))
        {
            return false;
        }

        return int.TryParse(value.ToString(), out userId);
    }
}