using System.Globalization;
using Tellurian.Trains.Communications.Interfaces.Extensions;

namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

public sealed class UnsupportedNotification : Notification
{
    private readonly byte[] _Data;

    internal UnsupportedNotification(byte[] data)
    {
        _Data = data;
    }
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnsupportedNotification, _Data.AsHex());
}
