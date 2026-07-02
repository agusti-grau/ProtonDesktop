using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Infrastructure.Data.Configurations;

public class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Uid).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Description);
        builder.Property(x => x.Location).HasMaxLength(500);
        builder.Property(x => x.RecurrenceRule);
        builder.Property(x => x.ETag).HasMaxLength(200);

        builder.HasOne(x => x.RecurrenceParent)
            .WithMany()
            .HasForeignKey(x => x.RecurrenceParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Reminders)
            .WithOne(x => x.CalendarEvent)
            .HasForeignKey(x => x.CalendarEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Uid);
        builder.HasIndex(x => new { x.CalendarId, x.Uid }).IsUnique();
        builder.HasIndex(x => x.StartUtc);
        builder.HasIndex(x => x.EndUtc);
        builder.HasIndex(x => x.DeletedAt);
    }
}
