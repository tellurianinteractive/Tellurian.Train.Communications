#pragma warning disable CA1051 // Do not declare visible instance fields

namespace Tellurian.Trains.Interfaces;

public abstract class Notification(DateTimeOffset timestamp)
{
    protected Notification() : this(DateTimeOffset.Now) { }

    protected Notification(DateTimeOffset timestamp, string message) : this(timestamp) => Message = message ?? string.Empty;

    public virtual bool IsLocoNotification { get; }

    public readonly DateTimeOffset Timestamp = timestamp;

    public readonly string? Message;

    public override string ToString() => $"{GetType().Name} {Message}";
}
