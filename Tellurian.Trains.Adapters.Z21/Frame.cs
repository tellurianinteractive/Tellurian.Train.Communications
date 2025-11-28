using Tellurian.Trains.Communications.Channels;

#pragma warning disable CA1819 // Properties should not return arrays

namespace Tellurian.Trains.Adapters.Z21;

public class Frame
{
    internal static IEnumerable<Frame> CreateMany(CommunicationResult result)
    {
        if (!result.IsSuccess) return Array.Empty<Frame>();
        var success = (SuccessResult)result;
        var buffer = success.Data();
        var offset = 0;
        var frames = new List<Frame>();
        while (offset < buffer.Length)
        {
            var frame = new Frame(buffer, offset, success.Timestamp);
            frames.Add(frame);
            offset += frame.Length;
        }
        return frames;
    }

    internal Frame(byte[] buffer, int offset, DateTimeOffset timestamp)
    {
        Timestamp = timestamp;
        Length = BitConverter.ToInt16(buffer, offset);
        Header = (FrameHeader)BitConverter.ToInt16(buffer, offset + 2);
        Data = new byte[Length - 4];
        Buffer.BlockCopy(buffer, offset + 4, Data, 0, Length - 4);
    }

    internal Frame(FrameHeader header) : this(header, Array.Empty<byte>()) { }

    internal Frame(FrameHeader header, byte[]? data)
    {
        Header = header;
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Length = data.Length + 4;
    }

    public DateTimeOffset Timestamp { get; }
    public int Length { get; }
    public FrameHeader Header { get; }
    public byte[] Data { get; }
    public byte[] GetBytes()
    {
        var buffer = new byte[Length];
        Buffer.BlockCopy(BitConverter.GetBytes((ushort)Length), 0, buffer, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes((ushort)Header), 0, buffer, 2, 2);
        Buffer.BlockCopy(Data, 0, buffer, 4, Data.Length);
        return buffer;
    }
}
