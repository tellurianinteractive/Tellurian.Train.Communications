using System.Runtime.CompilerServices;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

[assembly: InternalsVisibleTo("Tellurian.Trains.Protocols.LocoNet.Tests")]

namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// Base class for all LocoNet commands and notifications.
/// </summary>
public class Message
{
    /// <summary>
    /// Creates the appropriate type of <see cref="Message"/> from binary data.
    /// </summary>
    /// <param name="buffer">Complete LocoNet message including opcode and checksum</param>
    /// <returns>Parsed message object</returns>
    public static Message CreateMessage(byte[] buffer)
    {
        return LocoNetMessageFactory.Create(buffer);
    }

    public static byte Checksum(byte[] data)
    {
        if (data is null || data.Length == 0) return 0;
        var check = data[0];
        for (var i = 1; i < data.Length - 1; i++)
        {
            check ^= data[i];
        }
        return (byte)(~check);
    }

    public static byte[] AppendChecksum(byte[] dataWithoutChecksum)
    {
        if (dataWithoutChecksum is null) return [];
        var length = dataWithoutChecksum.Length;
        var result = new byte[length + 1];
        Array.Copy(dataWithoutChecksum, 0, result, 0, length);
        result[length] = Checksum(result);
        return result;
    }

    public static byte[] AppendChecksum(byte singleByteData)
    {
        return AppendChecksum([singleByteData]);
    }

    /// <summary>
    /// Gets the expected message length from an opcode byte.
    /// </summary>
    /// <param name="opcode">The opcode byte (must have MSB=1)</param>
    /// <returns>
    /// Message length in bytes, or -1 if variable length (need to read next byte for count)
    /// Returns 0 if the byte is not a valid opcode (MSB=0)
    /// </returns>
    public static int GetMessageLength(byte opcode)
    {
        // Opcode must have MSB (bit 7) set
        if ((opcode & 0x80) == 0) return 0;

        // Bits 6-5 encode the length
        int lengthBits = (opcode >> 5) & 0x03;

        return lengthBits switch
        {
            0b00 => 2,  // 2 bytes: opcode + checksum
            0b01 => 4,  // 4 bytes: opcode + 2 args + checksum
            0b10 => 6,  // 6 bytes: opcode + 4 args + checksum
            0b11 => -1, // Variable: next byte contains total byte count
            _ => 0
        };
    }

    /// <summary>
    /// Checks if an opcode has the follow-on bit set (expects a response).
    /// </summary>
    /// <param name="opcode">The opcode byte</param>
    /// <returns>True if the opcode expects a follow-on response message</returns>
    public static bool ExpectsResponse(byte opcode)
    {
        // Bit 3 is the follow-on bit
        return (opcode & 0x08) != 0;
    }

    public override string ToString()
    {
        return GetType().Name;
    }
}
