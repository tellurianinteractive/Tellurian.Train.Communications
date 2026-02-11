using System.Net;
using System.Net.Sockets;

namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Production implementation of <see cref="IUdpLocoNetAdapter"/> wrapping <see cref="UdpClient"/> with multicast support.
/// Supports both loconetd (Glenn Butcher) and GCA101 (Rocrail/Peter Giling) configurations.
/// </summary>
public sealed class UdpLocoNetAdapter(
    IPAddress multicastGroup,
    int listenPort,
    IPEndPoint sendEndpoint) : IUdpLocoNetAdapter
{
    private UdpClient? _client;
    private bool _disposed;

    /// <inheritdoc />
    public bool IsJoined { get; private set; }

    /// <inheritdoc />
    public string EndpointName { get; } = $"{multicastGroup}:{listenPort}";

    /// <inheritdoc />
    public Task JoinAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _client = new UdpClient(listenPort);
        _client.JoinMulticastGroup(multicastGroup);
        IsJoined = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<byte[]?> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_client is null) throw new InvalidOperationException("Must call JoinAsync before ReceiveAsync.");
        var result = await _client.ReceiveAsync(cancellationToken).ConfigureAwait(false);
        return result.Buffer;
    }

    /// <inheritdoc />
    public async Task SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_client is null) throw new InvalidOperationException("Must call JoinAsync before SendAsync.");
        await _client.SendAsync(data, sendEndpoint, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Close()
    {
        if (_client is not null)
        {
            try
            {
                _client.DropMulticastGroup(multicastGroup);
            }
            catch (SocketException)
            {
                // Ignore if already dropped or not joined
            }
            _client.Close();
        }
        IsJoined = false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            Close();
            _disposed = true;
        }
    }
}
