using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.Data.Configurations;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(AppUser.NameMaxLength);

        builder.Property(u => u.CreatedAt).IsRequired();
    }
}
