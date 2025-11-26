using Tellurian.Trains.Interfaces.Decoder;
using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Decoder;

/// <summary>
/// Base class for Operations Mode Programming (POM) commands.
/// POM allows CV modification while the locomotive is on the main track.
/// </summary>
/// <remarks>
/// Operations Mode Programming supports CV 1-1024.
/// The CV is encoded as a 10-bit value (0-1023) where user CV 1 maps to wire CV 0.
/// </remarks>
public abstract class ProgramOnMainCommand(byte[] data) : Command(0xE6, data)
{

    /// <summary>
    /// Validates that the CV number is in the valid range for POM (1-1024).
    /// </summary>
    protected static void ValidateCv(ushort cv)
    {
        if (cv < 1 || cv > 1024)
            throw new ArgumentOutOfRangeException(nameof(cv), "CV must be between 1 and 1024");
    }

    /// <summary>
    /// Encodes the CV number for the wire protocol.
    /// User CV 1-1024 maps to wire CV 0-1023.
    /// Returns the upper 2 bits (to be ORed with the mode byte) and lower 8 bits.
    /// </summary>
    protected static (byte upper2Bits, byte lower8Bits) EncodeCv(ushort cv)
    {
        var wireCv = (ushort)(cv - 1); // Convert 1-indexed to 0-indexed
        var upper = (byte)((wireCv >> 8) & 0x03);
        var lower = (byte)(wireCv & 0xFF);
        return (upper, lower);
    }
}

/// <summary>
/// Operations Mode Programming byte mode write request (spec section 4.23.1).
/// Writes a complete byte value to a CV while the locomotive is on the main track.
/// </summary>
/// <remarks>
/// Format: Header=0xE6, Data=[0x30, AH, AL, 0xEC+CC, CV, Value]
/// - AH/AL: Locomotive address (1-9999)
/// - CC: Upper 2 bits of CV (0-3)
/// - CV: Lower 8 bits of CV
/// - Value: The byte value to write
///
/// Note: XpressNet devices should not permit changes to the decoder's active locomotive address.
/// </remarks>
/// <remarks>
/// Creates a POM byte write command.
/// </remarks>
/// <param name="address">Locomotive address (1-9999)</param>
/// <param name="cv">CV with number (1-1024) and value (0-255)</param>
public sealed class ProgramOnMainWriteByteCommand(LocoAddress address, CV cv) : ProgramOnMainCommand(GetData(address, (ushort)cv.Number, cv.Value))
{
    private static byte[] GetData(LocoAddress address, ushort cv, byte value)
    {
        ValidateCv(cv);
        var (cvUpper, cvLower) = EncodeCv(cv);
        var addrBytes = address.GetBytesAccordingToXpressNet();

        return
        [
            0x30,                    // Identification
            addrBytes[0],            // Address High
            addrBytes[1],            // Address Low
            (byte)(0xEC + cvUpper),  // Mode byte + CV upper 2 bits
            cvLower,                 // CV lower 8 bits
            value                    // Data value
        ];
    }
}

/// <summary>
/// Operations Mode Programming bit mode write request (spec section 4.23.2).
/// Writes a single bit value to a CV while the locomotive is on the main track.
/// </summary>
/// <remarks>
/// Format: Header=0xE6, Data=[0x30, AH, AL, 0xE8+CC, CV, 0xF0+W*8+BBB]
/// - AH/AL: Locomotive address (1-9999)
/// - CC: Upper 2 bits of CV (0-3)
/// - CV: Lower 8 bits of CV
/// - W: Bit value (0 or 1)
/// - BBB: Bit position (0-7)
///
/// Note: XpressNet devices should not permit changes to the decoder's active locomotive address.
/// </remarks>
/// <remarks>
/// Creates a POM bit write command.
/// </remarks>
/// <param name="address">Locomotive address (1-9999)</param>
/// <param name="cvNumber">CV number (1-1024)</param>
/// <param name="bitPosition">Bit position within the CV (0-7, where 0 is LSB)</param>
/// <param name="bitValue">Bit value to write (true=1, false=0)</param>
public sealed class ProgramOnMainWriteBitCommand(LocoAddress address, ushort cvNumber, byte bitPosition, bool bitValue) : ProgramOnMainCommand(GetData(address, cvNumber, bitPosition, bitValue))
{
    private static byte[] GetData(LocoAddress address, ushort cvNumber, byte bitPosition, bool bitValue)
    {
        ValidateCv(cvNumber);
        if (bitPosition > 7)
            throw new ArgumentOutOfRangeException(nameof(bitPosition), "Bit position must be between 0 and 7");

        var (cvUpper, cvLower) = EncodeCv(cvNumber);
        var addrBytes = address.GetBytesAccordingToXpressNet();
        var bitByte = (byte)(0xF0 + (bitValue ? 0x08 : 0x00) + bitPosition);

        return
        [
            0x30,                    // Identification
            addrBytes[0],            // Address High
            addrBytes[1],            // Address Low
            (byte)(0xE8 + cvUpper),  // Mode byte + CV upper 2 bits
            cvLower,                 // CV lower 8 bits
            bitByte                  // Bit value and position
        ];
    }
}
