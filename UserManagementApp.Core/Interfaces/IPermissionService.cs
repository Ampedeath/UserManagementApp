
namespace UserManagementApp.Core.Interfaces;

public interface IPermissionService
{
    Task<bool> IsAdminAsync(int userId);
}