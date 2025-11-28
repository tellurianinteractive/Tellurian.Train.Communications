namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Delegate for reading a single byte from a byte stream.
/// </summary>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The byte read, or -1 if the read timed out or failed.</returns>
public delegate ValueTask<int> ReadByteDelegate(CancellationToken cancellationToken);

/// <summary>
/// Interface for assembling complete messages from a byte stream.
/// Different protocols can implement this interface to provide protocol-specific framing logic.
/// </summary>
public interface IByteStreamFramer
{
    /// <summary>
    /// Gets the protocol name for this framer (used in CommunicationResult).
    /// </summary>
    string ProtocolName { get; }

    /// <summary>
    /// Reads bytes from the stream until a complete message is assembled.
    /// </summary>
    /// <param name="readByte">Delegate to read a single byte from the underlying stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Complete message bytes if successful, or null if:
    /// - The read timed out before completing a message
    /// - The operation was cancelled
    /// - A sync error occurred and the framer needs to resync
    /// </returns>
    Task<byte[]?> ReadMessageAsync(ReadByteDelegate readByte, CancellationToken cancellationToken = default);
}
