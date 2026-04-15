namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// LAN_X_TURNOUT_INFO (X-Header 0x43). Reports the status of a turnout.
/// Broadcast by the Z21 when a turnout is switched by any client/handset, provided the
/// <c>RunningAndSwitching</c> (0x00000001) broadcast flag is subscribed. Also returned
/// as the reply to <c>LAN_X_GET_TURNOUT_INFO</c>.
/// </summary>
/// <remarks>
/// Frame layout (after Z21 framing strips DataLen/Header/XOR):
/// <c>0x43, FAdr_MSB, FAdr_LSB, 000000ZZ</c>.
/// The function address <c>FAdr</c> is the 0-based wire address — use
/// <see cref="Communications.Interfaces.Accessories.Address.FromWireAddress"/> to convert.
/// </remarks>
public sealed class TurnoutInfoNotification : Notification
{
    internal TurnoutInfoNotification(byte[] buffer) : base(0x43, GetPayload(buffer)) { }

    private static byte[] GetPayload(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Length < 4) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer must contain at least 4 bytes for LAN_X_TURNOUT_INFO.");
        return [buffer[1], buffer[2], buffer[3]];
    }

    /// <summary>
    /// The 0-based wire function address (FAdr_MSB &lt;&lt; 8 | FAdr_LSB).
    /// </summary>
    public ushort WireAddress => (ushort)((Data[0] << 8) | Data[1]);

    /// <summary>
    /// The turnout position indicator (bits 0-1 of DB2).
    /// </summary>
    public TurnoutPosition Position => (TurnoutPosition)(Data[2] & 0x03);
}

/// <summary>
/// Turnout position as reported by <see cref="TurnoutInfoNotification"/>.
/// </summary>
public enum TurnoutPosition : byte
{
    /// <summary>Turnout has not been switched since power-up.</summary>
    NotSwitched = 0x00,
    /// <summary>Last command was P=0 (Output 1 active — convention: closed/straight).</summary>
    Output1 = 0x01,
    /// <summary>Last command was P=1 (Output 2 active — convention: thrown/diverging).</summary>
    Output2 = 0x02,
    /// <summary>Invalid — both end switches reported active.</summary>
    Invalid = 0x03
}
