using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Infrastructure.Data.Configurations;

public class CalendarConfiguration : IEntityTypeConfiguration<Calendar>
{
    public void Configure(EntityTypeBuilder<Calendar> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Color).HasMaxLength(20);
        builder.Property(x => x.SyncToken).HasMaxLength(500);

        builder.HasMany(x => x.Events)
            .WithOne(x => x.Calendar)
            .HasForeignKey(x => x.CalendarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.MailAccountId, x.Name }).IsUnique();
    }
}
