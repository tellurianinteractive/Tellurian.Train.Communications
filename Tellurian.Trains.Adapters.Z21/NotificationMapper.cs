using Tellurian.Trains.Communications.Interfaces.Detectors;
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
        {typeof(LocoNetNotification), MapLocoNetNotification},
        {typeof(LocoNetDetectorNotification), MapLocoNetDetectorNotification},
        {typeof(CanDetectorNotification), MapCanDetectorNotification}
    };

    public static Tellurian.Trains.Communications.Interfaces.Notification[] Map(this Notification notification)
    {
        if (notification is null) return [];
        var key = notification.GetType();
        if (Mappings.TryGetValue(key, out Func<Notification, Communications.Interfaces.Notification[]>? value)) return value.Invoke(notification);
        return MapDefaults.CreateUnmapped(notification.ToString());
    }

    private static Tellurian.Trains.Communications.Interfaces.Notification[] MapXpressnetNotification(Notification notification) =>
        ((XpressNetNotification)notification).Notification.Map();

    private static Tellurian.Trains.Communications.Interfaces.Notification[] MapLocoNetNotification(Notification notification) =>
        ((LocoNetNotification)notification).Message?.Map ?? [];

    private static Tellurian.Trains.Communications.Interfaces.Notification[] MapLocoNetDetectorNotification(Notification notification)
    {
        var n = (LocoNetDetectorNotification)notification;
        return n.DetectorType switch
        {
            0x01 or 0x11 => [new OccupancyNotification(n.FeedbackAddress, n.IsOccupied)],
            0x02 or 0x03 => [new TransponderNotification(n.FeedbackAddress, n.TransponderAddress, n.IsEntering)],
            0x10 => [new RailComLocomotiveNotification(n.FeedbackAddress, n.LocoAddress, n.HasDirection, n.IsForward, n.ClassInfo)],
            _ => MapDefaults.CreateUnmapped(n.ToString()),
        };
    }

    private static Tellurian.Trains.Communications.Interfaces.Notification[] MapCanDetectorNotification(Notification notification)
    {
        var n = (CanDetectorNotification)notification;
        var feedbackAddress = (ushort)((n.ModuleAddress * 8) + n.Port);

        if (n.DetectorType == 0x01)
            return [new OccupancyNotification(feedbackAddress, n.IsOccupied)];

        if (n.IsRailCom)
        {
            var results = new List<Tellurian.Trains.Communications.Interfaces.Notification>();
            if (n.LocoAddress1 != 0)
                results.Add(new TransponderNotification(feedbackAddress, n.LocoAddress1, true));
            if (n.LocoAddress2 != 0)
                results.Add(new TransponderNotification(feedbackAddress, n.LocoAddress2, true));
            return results.Count > 0 ? [.. results] : MapDefaults.CreateUnmapped(n.ToString());
        }

        return MapDefaults.CreateUnmapped(n.ToString());
    }
}
