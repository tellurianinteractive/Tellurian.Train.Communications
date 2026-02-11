using System.Net.Sockets;
using System.Text;

namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Production implementation of <see cref="ITcpStreamAdapter"/> wrapping a <see cref="TcpClient"/>.
/// </summary>
/// <param name="hostname">The hostname or IP address of the remote server.</param>
/// <param name="port">The TCP port. Default is 1234 (LoconetOverTcp standard port).</param>
public sealed class TcpStreamAdapter(string hostname, int port = 1234) : ITcpStreamAdapter
{
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private bool _disposed;

    /// <inheritdoc />
    public bool IsConnected => _client?.Connected == true;

    /// <inheritdoc />
    public string RemoteEndpointName { get; } = $"{hostname}:{port}";

    /// <inheritdoc />
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _client = new TcpClient();
        await _client.ConnectAsync(hostname, port, cancellationToken).ConfigureAwait(false);
        var stream = _client.GetStream();
        _reader = new StreamReader(stream, Encoding.ASCII);
        _writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
    }

    /// <inheritdoc />
    public async Task<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_reader is null) return null;
        return await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task WriteLineAsync(string line, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_writer is null) return;
        await _writer.WriteLineAsync(line.AsMemory(), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Close()
    {
        _reader?.Dispose();
        _writer?.Dispose();
        _client?.Close();
        _reader = null;
        _writer = null;
        _client = null;
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
