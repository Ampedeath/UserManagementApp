using Microsoft.AspNetCore.Mvc;
using UserManagementApp.Core.DTOs.Auth;
using UserManagementApp.Core.Interfaces;

namespace UserManagementApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var login = await _authService.LoginAsync(request);
        
        if  (login == null)
        {
            return Unauthorized(new
            {
                Message = "Invalid username or password."
            });
        }
        return Ok(login);
    }
}