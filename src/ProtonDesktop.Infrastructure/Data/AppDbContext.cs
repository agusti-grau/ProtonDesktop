using Microsoft.EntityFrameworkCore;
using ProtonDesktop.Core.Models;
using ProtonDesktop.Infrastructure.Data.Configurations;

namespace ProtonDesktop.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<MailAccount> MailAccounts => Set<MailAccount>();
    public DbSet<EmailFolder> EmailFolders => Set<EmailFolder>();
    public DbSet<EmailMessage> EmailMessages => Set<EmailMessage>();
    public DbSet<EmailAttachment> EmailAttachments => Set<EmailAttachment>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Calendar> Calendars => Set<Calendar>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<CalendarReminder> CalendarReminders => Set<CalendarReminder>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new MailAccountConfiguration());
        modelBuilder.ApplyConfiguration(new EmailFolderConfiguration());
        modelBuilder.ApplyConfiguration(new EmailMessageConfiguration());
        modelBuilder.ApplyConfiguration(new EmailAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new ContactConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarEventConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarReminderConfiguration());
    }
}
