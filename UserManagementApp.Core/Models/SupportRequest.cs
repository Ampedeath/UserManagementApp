using UserManagementApp.Core.Enums;

namespace UserManagementApp.Core.Models;

public class SupportRequest
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public SupportRequestStatus Status { get; set; } = SupportRequestStatus.Open;

    public DateTime CreatedAt { get; set; }
}