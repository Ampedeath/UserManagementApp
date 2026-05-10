using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementApp.Data.Database;

namespace UserManagementApp.Api.Controllers;

[ApiController]
[Route("api/database")]
public class DatabaseController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public DatabaseController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetDatabaseStatus()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();

            return Ok(new
            {
                Status = canConnect ? "Connected" : "Disconnected",
                CanConnect = canConnect,
                Provider = _dbContext.Database.ProviderName
            });
        }
        catch (Exception exception)
        {
            return StatusCode(500, new
            {
                Status = "Error",
                CanConnect = false,
                Error = exception.Message
            });
        }
    }
}