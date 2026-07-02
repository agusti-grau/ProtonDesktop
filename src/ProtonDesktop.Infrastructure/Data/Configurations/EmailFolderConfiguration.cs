using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Infrastructure.Data.Configurations;

public class EmailFolderConfiguration : IEntityTypeConfiguration<EmailFolder>
{
    public void Configure(EntityTypeBuilder<EmailFolder> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Path).IsRequired().HasMaxLength(500);
        builder.Property(x => x.UidNext).HasMaxLength(100);
        builder.Property(x => x.UidValidity).HasMaxLength(100);

        builder.HasOne(x => x.ParentFolder)
            .WithMany(x => x.SubFolders)
            .HasForeignKey(x => x.ParentFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Folder)
            .HasForeignKey(x => x.FolderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.MailAccountId, x.Path }).IsUnique();
        builder.HasIndex(x => x.FolderType);
    }
}
