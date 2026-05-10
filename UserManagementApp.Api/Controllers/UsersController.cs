using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using UserManagementApp.Core.DTOs.Users;
using UserManagementApp.Core.Interfaces;
using UserManagementApp.Core.Models;
using UserManagementApp.Data.Database;

namespace UserManagementApp.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
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

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(int id, UpdateUserRequest request)
    {
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
    public async Task<IActionResult> DeleteUser(int id)
    {
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
}