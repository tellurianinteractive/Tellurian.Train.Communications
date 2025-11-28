using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Locomotive is being operated by another device response (spec section 3.15).
/// Sent unrequested when another XpressNet device takes control of a locomotive.
/// </summary>
/// <remarks>
/// Format: Header=0xE3, Data=[0x40, AH, AL]
///
/// This notification is sent to the XpressNet device that previously had control
/// of a locomotive when another device takes over using a Locomotive operations request.
/// </remarks>
public sealed class LocoOperatedByAnotherDeviceNotification : Notification
{
    internal LocoOperatedByAnotherDeviceNotification(byte[] buffer) : base(0xE3, GetData(buffer)) { }

    private static new byte[] GetData(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Length < 4) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer must contain at least 4 bytes");
        return [buffer[1], buffer[2], buffer[3]];
    }

    /// <summary>
    /// Gets the address high byte.
    /// </summary>
    public byte AddressHigh => Data[1];

    /// <summary>
    /// Gets the address low byte.
    /// </summary>
    public byte AddressLow => Data[2];

    /// <summary>
    /// Gets the locomotive address that is now being operated by another device.
    /// </summary>
    public LocoAddress LocoAddress => Interfaces.Locos.LocoAddress.From([AddressHigh, AddressLow]);
}
