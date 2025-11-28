using System.IO.Ports;

namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Adapter that wraps <see cref="SerialPort"/> and implements <see cref="ISerialPortAdapter"/>.
/// </summary>
public sealed class SerialPortAdapter : ISerialPortAdapter
{
    private readonly SerialPort _serialPort;
    private bool _disposed;

    /// <summary>
    /// Creates a new serial port adapter.
    /// </summary>
    /// <param name="portName">The name of the serial port (e.g., "COM3").</param>
    /// <param name="baudRate">The baud rate. Default is 57600 (common for LocoNet USB adapters).</param>
    /// <param name="parity">The parity. Default is None.</param>
    /// <param name="dataBits">The data bits. Default is 8.</param>
    /// <param name="stopBits">The stop bits. Default is One.</param>
    public SerialPortAdapter(
        string portName,
        int baudRate = 57600,
        Parity parity = Parity.None,
        int dataBits = 8,
        StopBits stopBits = StopBits.One)
    {
        _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
    }

    /// <inheritdoc />
    public bool IsOpen => _serialPort.IsOpen;

    /// <inheritdoc />
    public string PortName => _serialPort.PortName;

    /// <inheritdoc />
    public int ReadTimeout
    {
        get => _serialPort.ReadTimeout;
        set => _serialPort.ReadTimeout = value;
    }

    /// <inheritdoc />
    public void Open()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _serialPort.Open();
    }

    /// <inheritdoc />
    public void Close()
    {
        if (!_disposed && _serialPort.IsOpen)
        {
            _serialPort.Close();
        }
    }

    /// <inheritdoc />
    public async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (data is null || data.Length == 0) return;

        await _serialPort.BaseStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
        await _serialPort.BaseStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<int> ReadByteAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var buffer = new byte[1];
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (_serialPort.ReadTimeout > 0)
            {
                timeoutCts.CancelAfter(_serialPort.ReadTimeout);
            }

            var bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, timeoutCts.Token).ConfigureAwait(false);
            return bytesRead == 1 ? buffer[0] : -1;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred (not user cancellation)
            return -1;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _serialPort.Dispose();
            _disposed = true;
        }
    }
}
