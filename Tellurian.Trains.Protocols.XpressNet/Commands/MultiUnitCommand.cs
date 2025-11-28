using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Add a locomotive to a Multi-Unit command (spec section 4.24.1).
/// Adds a locomotive to a Multi-Unit (MTR) consist.
/// </summary>
/// <remarks>
/// Format: Header=0xE4, Data=[0x40+R, AH, AL, MTR]
/// - R=0: Locomotive direction same as consist direction
/// - R=1: Locomotive direction reversed relative to consist direction
/// - AH/AL: Locomotive address (1-9999)
/// - MTR: Multi-Unit base address (1-99)
///
/// If this is a new consist, a new entry is created automatically.
/// A locomotive cannot be added to a multi-unit with the same address as itself.
///
/// Response: If not successful, the command station sends MUDHErrorNotification (section 3.21).
/// </remarks>
public sealed class AddLocoToMultiUnitCommand : Command
{
    /// <summary>
    /// Creates a command to add a locomotive to a Multi-Unit consist.
    /// </summary>
    /// <param name="locoAddress">Locomotive address to add (1-9999)</param>
    /// <param name="multiUnitAddress">Multi-Unit base address (1-99)</param>
    /// <param name="reversed">True if locomotive direction is reversed relative to consist</param>
    public AddLocoToMultiUnitCommand(LocoAddress locoAddress, byte multiUnitAddress, bool reversed = false)
        : base(0xE4, GetData(locoAddress, multiUnitAddress, reversed))
    {
        ValidateMultiUnitAddress(multiUnitAddress);
    }

    private static byte[] GetData(LocoAddress locoAddress, byte multiUnitAddress, bool reversed)
    {
        var addrBytes = locoAddress.GetBytesAccordingToXpressNet();
        return
        [
            (byte)(0x40 + (reversed ? 1 : 0)),  // Identification with direction bit
            addrBytes[0],                        // Loco Address High
            addrBytes[1],                        // Loco Address Low
            multiUnitAddress                     // MTR address
        ];
    }

    private static void ValidateMultiUnitAddress(byte address)
    {
        if (address < 1 || address > 99)
            throw new ArgumentOutOfRangeException(nameof(address), "Multi-Unit address must be between 1 and 99");
    }
}

/// <summary>
/// Remove a locomotive from a Multi-Unit command (spec section 4.24.2).
/// Removes a locomotive from a Multi-Unit (MTR) consist.
/// </summary>
/// <remarks>
/// Format: Header=0xE4, Data=[0x42, AH, AL, MTR]
/// - AH/AL: Locomotive address (1-9999)
/// - MTR: Multi-Unit base address (1-99)
///
/// If the locomotive is the last one in the consist, the consist entry is deleted automatically.
///
/// Response: If not successful, the command station sends MUDHErrorNotification (section 3.21).
/// </remarks>
public sealed class RemoveLocoFromMultiUnitCommand : Command
{
    /// <summary>
    /// Creates a command to remove a locomotive from a Multi-Unit consist.
    /// </summary>
    /// <param name="locoAddress">Locomotive address to remove (1-9999)</param>
    /// <param name="multiUnitAddress">Multi-Unit base address (1-99)</param>
    public RemoveLocoFromMultiUnitCommand(LocoAddress locoAddress, byte multiUnitAddress)
        : base(0xE4, GetData(locoAddress, multiUnitAddress))
    {
        ValidateMultiUnitAddress(multiUnitAddress);
    }

    private static byte[] GetData(LocoAddress locoAddress, byte multiUnitAddress)
    {
        var addrBytes = locoAddress.GetBytesAccordingToXpressNet();
        return
        [
            0x42,               // Identification for remove
            addrBytes[0],       // Loco Address High
            addrBytes[1],       // Loco Address Low
            multiUnitAddress    // MTR address
        ];
    }

    private static void ValidateMultiUnitAddress(byte address)
    {
        if (address < 1 || address > 99)
            throw new ArgumentOutOfRangeException(nameof(address), "Multi-Unit address must be between 1 and 99");
    }
}
