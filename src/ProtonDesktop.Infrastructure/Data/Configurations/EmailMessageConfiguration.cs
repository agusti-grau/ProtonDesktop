using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Infrastructure.Data.Configurations;

public class EmailMessageConfiguration : IEntityTypeConfiguration<EmailMessage>
{
    public void Configure(EntityTypeBuilder<EmailMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MessageId).IsRequired().HasMaxLength(500);
        builder.Property(x => x.InReplyTo).HasMaxLength(500);
        builder.Property(x => x.Subject).HasMaxLength(1000);
        builder.Property(x => x.FromAddress).IsRequired().HasMaxLength(255);
        builder.Property(x => x.FromName).HasMaxLength(255);
        builder.Property(x => x.ToAddresses).IsRequired();
        builder.Property(x => x.CcAddresses);
        builder.Property(x => x.BccAddresses);
        builder.Property(x => x.Uid).HasMaxLength(100);

        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.EmailMessage)
            .HasForeignKey(x => x.EmailMessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MessageId);
        builder.HasIndex(x => new { x.FolderId, x.Uid }).IsUnique();
        builder.HasIndex(x => x.ReceivedAt);
        builder.HasIndex(x => x.FromAddress);
        builder.HasIndex(x => x.DeletedAt);
    }
}
