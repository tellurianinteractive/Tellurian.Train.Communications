namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Abstraction over a TCP stream for testability.
/// </summary>
public interface ITcpStreamAdapter : IDisposable
{
    /// <summary>
    /// Gets whether the TCP connection is established.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets a display name for the remote endpoint (e.g., "192.168.1.100:1234").
    /// </summary>
    string RemoteEndpointName { get; }

    /// <summary>
    /// Connects to the remote server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a line of text from the stream.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The line read, or null if the connection was closed.</returns>
    Task<string?> ReadLineAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a line of text to the stream.
    /// </summary>
    /// <param name="line">The line to write.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task WriteLineAsync(string line, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the connection.
    /// </summary>
    void Close();
}
