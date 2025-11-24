using Tellurian.Trains.Protocols.XpressNet;

namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Wraps received Xpressnet/XBUS notification.
/// </summary>
public sealed class XpressNetNotification : Notification
{
    internal XpressNetNotification(Frame frame) : base(frame)
    {
        var packet = new Packet(frame.Data);
        Notification = packet.Notification;
    }
    public Protocols.XpressNet.Notifications.Notification Notification { get; }
}
