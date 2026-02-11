namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Abstraction over multicast UDP for testability.
/// </summary>
public interface IUdpLocoNetAdapter : IDisposable
{
    /// <summary>
    /// Gets whether the multicast group has been joined and we're listening.
    /// </summary>
    bool IsJoined { get; }

    /// <summary>
    /// Gets a display name for the endpoint (e.g., "225.0.0.2:4501").
    /// </summary>
    string EndpointName { get; }

    /// <summary>
    /// Creates UDP client, binds listen port, and joins the multicast group.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task JoinAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Receives a UDP datagram, returning the raw bytes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The received bytes, or null if the connection was closed.</returns>
    Task<byte[]?> ReceiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends raw bytes to the configured send endpoint.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAsync(byte[] data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Drops multicast membership and closes the UDP client.
    /// </summary>
    void Close();
}
