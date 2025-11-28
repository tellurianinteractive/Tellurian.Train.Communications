namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Abstraction over a serial port for testability.
/// </summary>
public interface ISerialPortAdapter : IDisposable
{
    /// <summary>
    /// Gets whether the serial port is open.
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// Gets the name of the serial port (e.g., "COM3").
    /// </summary>
    string PortName { get; }

    /// <summary>
    /// Opens the serial port.
    /// </summary>
    void Open();

    /// <summary>
    /// Closes the serial port.
    /// </summary>
    void Close();

    /// <summary>
    /// Writes data to the serial port.
    /// </summary>
    /// <param name="data">The data to write.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task WriteAsync(byte[] data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a single byte from the serial port.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The byte read, or -1 if the read timed out.</returns>
    ValueTask<int> ReadByteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets the read timeout in milliseconds.
    /// </summary>
    int ReadTimeout { get; set; }
}
