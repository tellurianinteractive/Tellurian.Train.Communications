using System.Globalization;
using Tellurian.Trains.Interfaces.Extensions;

namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

public sealed class SlotNotification : Notification
{
    private readonly byte[] _Data;
    internal SlotNotification(byte[] data)
    {
        _Data = data;
    }
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, Resources.Strings.SlotNotification, _Data.AsHex());
}
