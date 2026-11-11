using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data.Entities;
using TodoApp.Api.Services;

namespace TodoApp.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<TodoTask> TodoTasks => Set<TodoTask>();

    public DbSet<SubTask> SubTasks => Set<SubTask>();

    /// <summary>
    /// The authenticated user's id from the JWT. Used by the ownership query filters (AD-5).
    /// </summary>
    public string? CurrentUserId => currentUserService.UserId;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Ownership + soft-delete filters (AD-5, NFR-7, NFR-16).
        builder.Entity<Category>().HasQueryFilter(c => c.UserId == CurrentUserId && !c.IsDeleted);
        builder.Entity<TodoTask>().HasQueryFilter(t => t.UserId == CurrentUserId && !t.IsDeleted);
        builder.Entity<SubTask>().HasQueryFilter(s => !s.IsDeleted);
    }

    private void ApplyAuditAndSoftDelete()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.Entity)
            {
                case TodoTask task when entry.State == EntityState.Added:
                    task.CreatedAt = utcNow;
                    task.UpdatedAt = utcNow;
                    break;
                case TodoTask task when entry.State == EntityState.Modified:
                    task.UpdatedAt = utcNow;
                    break;
                case AppUser user when entry.State == EntityState.Added:
                    user.CreatedAt = utcNow;
                    break;
            }

            if (entry is { State: EntityState.Deleted, Entity: ISoftDeletable softDeletable })
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = utcNow;
            }
        }
    }
}
