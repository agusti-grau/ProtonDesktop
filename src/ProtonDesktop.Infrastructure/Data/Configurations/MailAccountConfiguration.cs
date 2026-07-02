using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Infrastructure.Data.Configurations;

public class MailAccountConfiguration : IEntityTypeConfiguration<MailAccount>
{
    public void Configure(EntityTypeBuilder<MailAccount> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.Property(x => x.DisplayName).HasMaxLength(255);
        builder.Property(x => x.ImapHost).HasMaxLength(255);
        builder.Property(x => x.SmtpHost).HasMaxLength(255);
        builder.Property(x => x.CalDavHost).HasMaxLength(255);
        builder.Property(x => x.EncryptedPassword).IsRequired();

        builder.HasMany(x => x.Folders)
            .WithOne(x => x.MailAccount)
            .HasForeignKey(x => x.MailAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Calendars)
            .WithOne(x => x.MailAccount)
            .HasForeignKey(x => x.MailAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.IsDefault);
    }
}
