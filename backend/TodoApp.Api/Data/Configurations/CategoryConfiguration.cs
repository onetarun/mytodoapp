using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.Data.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId).IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(Category.NameMaxLength);

        // Users are never hard deleted in v1; Restrict prevents an accidental delete from wiping data.
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.UserId);
    }
}
