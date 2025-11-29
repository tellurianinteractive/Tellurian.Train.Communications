using Tellurian.Trains.Interfaces.Decoder;
using Tellurian.Trains.Interfaces.Extensions;
using Tellurian.Trains.Interfaces.Locos;
using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

public abstract class Notification : Message
{
    protected Notification(byte header) : base(header) { }
    protected Notification(byte header, byte data) : base(header, data) { }
    protected Notification(byte header, byte[] data) : base(header, data) { }

    protected Notification(Func<byte[], byte> headerFunc, byte[] buffer) : base(headerFunc, buffer) { }
    protected Notification(Func<byte[], byte> headerFunc, byte[] buffer, Func<byte[], bool> lengthFunc) : base(headerFunc, buffer, lengthFunc) { }
}

public static class NotificationExtensions
{
    private static readonly IDictionary<Type, Func<Notification, Interfaces.Notification[]>> Mappings = new Dictionary<Type, Func<Notification, Interfaces.Notification[]>>()
    {
        { typeof(LocoInfoNotification), MapLocoInfoNotification },
        { typeof(WriteCVResponse), MapDecoderResponse }
    };

    public static Interfaces.Notification[] Map(this Notification notification)
    {
        var key = notification?.GetType().Key() ?? throw new ArgumentNullException(nameof(notification));
        if (key is null) return MapDefaults.CreateUnmapped(notification.ToString());
        return Mappings[key].Invoke(notification);
    }

    private static Type Key(this Type type) => Mappings.Keys.SingleOrDefault(k => k.Equals(type) || type.IsSubclassOf(k)) ?? throw new InvalidOperationException(type.Name);

    private static Interfaces.Notification[] MapLocoInfoNotification(Notification notification)
    {
        var n = (LocoInfoNotification)notification;
        var result = new Interfaces.Notification[2];
        result[0] = new LocoMovementNotification(n.Address, n.Direction.Map(), n.Speed.Map());
        result[1] = new LocoFunctionsNotification(n.Address, n.Functions().Map());
        return result;
    }

    private static Interfaces.Notification[] MapDecoderResponse(Notification notification) =>
        [
            notification switch
            {
                CVOkResponse ok => DecoderResponse.Success(ok.CV),
                WriteCVTimeoutResponse => DecoderResponse.Timeout(),
                WriteCVShortCircuitResponse => DecoderResponse.Shortcircuit(),
                _ => throw new NotSupportedException()
            }
        ];
}
