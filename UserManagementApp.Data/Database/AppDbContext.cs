using Microsoft.EntityFrameworkCore;
using UserManagementApp.Core.Models;

namespace UserManagementApp.Data.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<SupportRequest> SupportRequests => Set<SupportRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigureUser(modelBuilder);
        ConfigureSupportRequest(modelBuilder);
    }
    
    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(user => user.PasswordHash)
                .IsRequired();

            entity.Property(user => user.Role)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(user => user.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(user => user.LastName)
                .HasMaxLength(50);

            entity.Property(user => user.CreatedAt)
                .IsRequired();

            entity.Property(user => user.UpdatedAt)
                .IsRequired(false);

            entity.HasIndex(user => user.Username)
                .IsUnique();

            entity.HasIndex(user => user.Email)
                .IsUnique();
        });
    }
    
    private static void ConfigureSupportRequest(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SupportRequest>(entity =>
        {
            entity.HasKey(request => request.Id);

            entity.Property(request => request.Subject)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(request => request.Message)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(request => request.Status)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(request => request.CreatedAt)
                .IsRequired();

            entity.HasOne(request => request.User)
                .WithMany(user => user.SupportRequests)
                .HasForeignKey(request => request.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}