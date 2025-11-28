using Tellurian.Trains.Communications.Channels;

namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// Message framer for LocoNet protocol.
/// Handles byte-by-byte reading from a serial port and assembles complete LocoNet messages.
/// </summary>
public sealed class LocoNetFramer : IByteStreamFramer
{
    private readonly int _interByteTimeoutMs;
    private readonly bool _validateChecksum;

    /// <summary>
    /// Creates a new LocoNet framer.
    /// </summary>
    /// <param name="interByteTimeoutMs">
    /// Timeout in milliseconds between bytes within a message.
    /// If a byte is not received within this time, the framer resyncs.
    /// Default is 100ms.
    /// </param>
    /// <param name="validateChecksum">
    /// Whether to validate the checksum of received messages.
    /// If validation fails, the message is discarded and framer resyncs.
    /// Default is true.
    /// </param>
    public LocoNetFramer(int interByteTimeoutMs = 100, bool validateChecksum = true)
    {
        _interByteTimeoutMs = interByteTimeoutMs;
        _validateChecksum = validateChecksum;
    }

    /// <inheritdoc />
    public string ProtocolName => "LocoNet";

    /// <inheritdoc />
    public async Task<byte[]?> ReadMessageAsync(ReadByteDelegate readByte, CancellationToken cancellationToken = default)
    {
        // Step 1: Find an opcode (byte with MSB=1)
        int opcodeByte;
        do
        {
            opcodeByte = await readByte(cancellationToken).ConfigureAwait(false);
            if (opcodeByte < 0) return null; // Timeout or cancelled
        }
        while ((opcodeByte & 0x80) == 0); // Skip bytes until we find MSB=1

        var opcode = (byte)opcodeByte;

        // Step 2: Determine message length from opcode
        var length = Message.GetMessageLength(opcode);
        if (length == 0)
        {
            // Invalid opcode (shouldn't happen if MSB check passed, but be safe)
            return null;
        }

        // Step 3: Handle variable length messages (length == -1)
        byte[]? buffer;
        if (length < 0)
        {
            // Variable length: byte 1 contains the count
            var countByte = await ReadByteWithTimeoutAsync(readByte, cancellationToken).ConfigureAwait(false);
            if (countByte < 0) return null; // Timeout - resync

            // In LocoNet, the count byte contains the total message length including opcode and checksum
            length = countByte & 0x7F;
            if (length < 2)
            {
                // Invalid length - resync
                return null;
            }

            buffer = new byte[length];
            buffer[0] = opcode;
            buffer[1] = (byte)countByte;

            // Read remaining bytes (length - 2 already read)
            for (int i = 2; i < length; i++)
            {
                var b = await ReadByteWithTimeoutAsync(readByte, cancellationToken).ConfigureAwait(false);
                if (b < 0) return null; // Timeout - resync
                buffer[i] = (byte)b;
            }
        }
        else
        {
            // Fixed length message
            buffer = new byte[length];
            buffer[0] = opcode;

            // Read remaining bytes (length - 1 already read)
            for (int i = 1; i < length; i++)
            {
                var b = await ReadByteWithTimeoutAsync(readByte, cancellationToken).ConfigureAwait(false);
                if (b < 0) return null; // Timeout - resync
                buffer[i] = (byte)b;
            }
        }

        // Step 4: Validate checksum if enabled
        if (_validateChecksum && !ValidateChecksum(buffer))
        {
            // Checksum failed - discard and resync
            return null;
        }

        return buffer;
    }

    private async ValueTask<int> ReadByteWithTimeoutAsync(ReadByteDelegate readByte, CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_interByteTimeoutMs);

        try
        {
            return await readByte(timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Inter-byte timeout (not user cancellation)
            return -1;
        }
    }

    private static bool ValidateChecksum(byte[] data)
    {
        if (data is null || data.Length < 2) return false;

        // XOR all bytes - result should be 0xFF for valid checksum
        byte check = 0;
        foreach (var b in data)
        {
            check ^= b;
        }
        return check == 0xFF;
    }
}
