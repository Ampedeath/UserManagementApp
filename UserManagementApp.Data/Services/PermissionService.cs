using Microsoft.EntityFrameworkCore;
using UserManagementApp.Core.Enums;
using UserManagementApp.Core.Interfaces;
using UserManagementApp.Data.Database;

namespace UserManagementApp.Data.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _dbContext;

    public PermissionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsAdminAsync(int userId)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.UserId == userId && user.Role == UserRole.Admin);
    }
}