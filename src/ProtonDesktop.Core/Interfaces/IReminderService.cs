namespace ProtonDesktop.Core.Interfaces;

public interface IReminderService
{
    Task StartAsync();
    Task StopAsync();
    event EventHandler<ReminderEventArgs> ReminderTriggered;
}

public class ReminderEventArgs : EventArgs
{
    public int EventId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public DateTime EventStart { get; init; }
    public int ReminderId { get; init; }
}
