using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Infrastructure.Data.Configurations;

public class CalendarReminderConfiguration : IEntityTypeConfiguration<CalendarReminder>
{
    public void Configure(EntityTypeBuilder<CalendarReminder> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.IsSent);
        builder.HasIndex(x => x.CalendarEventId);
    }
}
