using System.Globalization;
using System.Text.Json.Serialization;

#pragma warning disable CA1051 // Do not declare visible instance fields

namespace Tellurian.Trains.Protocols.XpressNet;

/// <summary>
/// Base class for all XpressNet protocol messages.
/// JSON serialization is handled by <see cref="Json.Converters.XpressNetMessageConverter"/>.
/// </summary>
public abstract class Message
{
    protected byte[] Data;

    [JsonPropertyName("header")]
    public byte Header { get; }

    [JsonPropertyName("length")]
    public int Length { get { return HasData ? Data.Length : 0; } }

    protected Message(byte header)
    {
        EnsureHeader(header);
        Header = header;
        Data = Array.Empty<byte>();
    }

    protected Message(byte header, byte data)
    {
        EnsureHeader(header);
        Header = header;
        Data = new byte[1];
        Data[0] = data;
    }
    protected Message(byte header, byte[] data)
    {
        EnsureHeader(header);
        Header = header;
        EnsureData(data);
        Data = data;
    }

    protected static byte[] GetData(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        var result = new byte[buffer.Length - 2];
        Array.Copy(buffer, 1, result, 0, result.Length);
        return result;
    }

    protected Message(Func<byte[], byte> headerFunc, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(headerFunc);
        EnsureData(buffer);
        var header = headerFunc.Invoke(buffer);
        EnsureHeader(header);
        Data = GetData(buffer);
    }

    protected Message(Func<byte[], byte> headerFunc, byte[] buffer, Func<byte[], bool> lengthFunc)
    {
        ArgumentNullException.ThrowIfNull(headerFunc);
        ArgumentNullException.ThrowIfNull(lengthFunc);
        EnsureData(buffer);
        if (!lengthFunc.Invoke(buffer)) throw new ArgumentOutOfRangeException(nameof(buffer), Resources.Strings.DataHasIncorrectLength);
        var header = headerFunc.Invoke(buffer);
        EnsureHeader(header);
        Header = header;
        Data = GetData(buffer);
    }

    private static void EnsureHeader(byte header)
    {
        if (header < 16)
            throw new ArgumentOutOfRangeException(nameof(header));
    }

    private static void EnsureData(byte[] data)
    {
        if (data == null || data.Length < 1) throw new ArgumentNullException(nameof(data));
        if (data.Length > 17) throw new ArgumentOutOfRangeException(nameof(data), Resources.Strings.DataLengthExceeded);
    }

    protected bool HasData { get { return !((Data == null) || (Data.Length == 0)); } }

    /// <summary>
    /// Gets the raw data bytes as a hex string for serialization/debugging.
    /// </summary>
    [JsonPropertyName("dataHex")]
    public string DataHex => HasData ? BitConverter.ToString(Data) : string.Empty;

    /// <summary>
    /// Gets the message type name for JSON serialization.
    /// </summary>
    [JsonPropertyName("messageType")]
    public string MessageType => GetType().Name;

    public override string ToString()
    {
        return GetType().Name;
    }

    public virtual string ToHex()
    {
        return string.Format(CultureInfo.InvariantCulture, "Header={0:X} Data={1}", Header, BitConverter.ToString(Data));
    }
}
