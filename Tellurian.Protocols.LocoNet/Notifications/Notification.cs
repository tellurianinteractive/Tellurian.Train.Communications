using System.Globalization;
using Tellurian.Trains.Interfaces.Extensions;

namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

/// <summary>
/// Base class for all messages notified from LocoNet.
/// </summary>
public abstract class Notification : Message
{
    protected static void ValidateData(byte expectedOperationCode, byte[] data)
    {
        if (data == null || data.Length == 0) throw new ArgumentNullException(nameof(data));
        if (data[0] != expectedOperationCode) throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnexpectedOperationCode, data[0].AsHex()));
        ValidateChecksum(data);

        static void ValidateChecksum(byte[] data)
        {
            var check = data[0];
            for (var i = 1; i < data.Length; i++) { check ^= data[i]; }
            if (check == 0xFF) return;
            throw new ArgumentOutOfRangeException(nameof(data), string.Format(CultureInfo.CurrentCulture, Resources.Strings.InvalidCheckSum, check.AsHex(), data.AsHex()));
        }
    }
}
