using Tellurian.Trains.Communications.Interfaces.Extensions;
using Tellurian.Trains.Communications.Interfaces.Locos;
using Tellurian.Trains.Protocols.LocoNet;
using Tellurian.Trains.Protocols.XpressNet;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Maps any message from Z21
/// </summary>
public static class NotificationMapper
{
    private static readonly IDictionary<Type, Func<Notification, Tellurian.Trains.Communications.Interfaces.Notification[]>> Mappings = new Dictionary<Type, Func<Notification, Tellurian.Trains.Communications.Interfaces.Notification[]>>()
    {
        {typeof(XpressNetNotification), MapXpressnetNotification },
        {typeof(LocoNetNotification), MapLocoNetNotification}
    };

    public static Tellurian.Trains.Communications.Interfaces.Notification[] Map(this Notification notification)
    {
        if (notification is null) return [];
        var key = notification.GetType();
        if (Mappings.ContainsKey(key)) return Mappings[key].Invoke(notification);
        return MapDefaults.CreateUnmapped(notification.ToString());
    }

    private static Tellurian.Trains.Communications.Interfaces.Notification[] MapXpressnetNotification(Notification notification) =>
        ((XpressNetNotification)notification).Notification.Map();

    private static Tellurian.Trains.Communications.Interfaces.Notification[] MapLocoNetNotification(Notification notification) =>
        ((LocoNetNotification)notification).Message?.Map ?? [];
}