using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.Data.Configurations;

public sealed class SubTaskConfiguration : IEntityTypeConfiguration<SubTask>
{
    public void Configure(EntityTypeBuilder<SubTask> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(SubTask.TitleMaxLength);

        builder.HasOne(s => s.TodoTask)
            .WithMany(t => t.SubTasks)
            .HasForeignKey(s => s.TodoTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.TodoTaskId);
    }
}
