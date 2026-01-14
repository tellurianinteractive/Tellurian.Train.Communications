using Tellurian.Trains.Communications.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Locomotive information response for address retrieval requests (spec section 3.17).
/// Sent as a response to address inquiry commands.
/// </summary>
/// <remarks>
/// Format: Header=0xE3, Data=[0x30+K, AH, AL]
///
/// K indicates the type of address returned:
/// - K=0: Normal locomotive address
/// - K=1: Locomotive is in a Double Header
/// - K=2: Multi-Unit base address
/// - K=3: Locomotive is in a Multi-Unit
/// - K=4: No address found (if AH/AL = 0x00)
/// </remarks>
public sealed class AddressRetrievalNotification : Notification
{
    internal AddressRetrievalNotification(byte[] buffer) : base(0xE3, GetData(buffer)) { }

    private static new byte[] GetData(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Length < 4) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer must contain at least 4 bytes");
        return [buffer[1], buffer[2], buffer[3]];
    }
    /// <summary>
    /// Gets a value indicating whether the address is valid and not set to the default or zero value.
    /// </summary>
    public bool HasValidAddress => AddressType != AddressType.Zero;

    /// <summary>
    /// Gets the address type code (K value, 0-4).
    /// </summary>
    public AddressType AddressType => (AddressType)(IdentificationByte & 0x0F);

    /// <summary>
    /// Gets the locomotive address if one was found.
    /// Returns null if no address was found.
    /// </summary>
    public Address LocoAddress
    {
        get
        {
            if (!HasValidAddress || (AddressHigh == 0 && AddressLow == 0))
                return Address.Zero;

            return LocoAddressExtensions.FromXpressNet(AddressHigh, AddressLow);
        }
    }

    private byte IdentificationByte => Data[0];
    private byte AddressHigh => Data[1];
    private byte AddressLow => Data[2];
}
