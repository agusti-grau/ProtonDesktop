using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Infrastructure.Data.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.Property(x => x.DisplayName).HasMaxLength(255);
        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.PhoneNumber).HasMaxLength(50);
        builder.Property(x => x.Company).HasMaxLength(255);

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => new { x.MailAccountId, x.Email }).IsUnique();
    }
}
