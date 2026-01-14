namespace Tellurian.Trains.Adapters.LocoNet.Tests;

/// <summary>
/// Test helper for observing notifications from the adapter.
/// </summary>
internal class TestNotificationObserver : IObserver<Tellurian.Trains.Communications.Interfaces.Notification>
{
    public int NotificationCount { get; private set; }
    public List<Tellurian.Trains.Communications.Interfaces.Notification> Notifications { get; } = [];
    public List<Exception> Errors { get; } = [];
    public bool IsCompleted { get; private set; }

    public void OnCompleted()
    {
        IsCompleted = true;
    }

    public void OnError(Exception error)
    {
        Errors.Add(error);
    }

    public void OnNext(Tellurian.Trains.Communications.Interfaces.Notification value)
    {
        NotificationCount++;
        Notifications.Add(value);
    }
}
