namespace Tellurian.Trains.Communications.Interfaces.Extensions;

public static class MapDefaults
{
    public static Notification[] CreateUnmapped(string message) => [new MessageNotification(DateTimeOffset.Now, message)];
}
