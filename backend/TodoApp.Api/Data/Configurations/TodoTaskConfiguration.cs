using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.Data.Configurations;

public sealed class TodoTaskConfiguration : IEntityTypeConfiguration<TodoTask>
{
    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).IsRequired();

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(TodoTask.TitleMaxLength);

        builder.Property(t => t.Description)
            .HasMaxLength(TodoTask.DescriptionMaxLength);

        // Stored as int. The default (Medium) is set by the entity initializer, not a database default,
        // so an explicit Low (0) is never replaced.
        builder.Property(t => t.Priority).HasConversion<int>();

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // NoAction in the database (avoids multiple cascade paths from AspNetUsers);
        // EF sets CategoryId to null on tracked tasks when their category is deleted.
        builder.HasOne(t => t.Category)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasIndex(t => new { t.UserId, t.IsDeleted, t.IsCompleted });
        builder.HasIndex(t => new { t.UserId, t.DueDate });
    }
}
