namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

public sealed class FeedbackBroadcast : Notification
{
    internal FeedbackBroadcast(byte[] buffer) : base(GetHeader, buffer, IsExpectedLength)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        Changed = Enumerable.Range(0, GetStatusCount(buffer) - 1)
            .Select(i => new AccessoryDecoderInfo(buffer[1 + (i * 2)], buffer[2 + (i * 2)])).ToArray();
    }

    private static byte GetHeader(byte[] buffer)
    {
        if (buffer[0] < 0x41 && buffer[0] > 0x47) throw new ArgumentOutOfRangeException(nameof(buffer), Resources.Strings.InvalidHeader);
        return buffer[0];
    }

    private static bool IsExpectedLength(byte[] buffer)
    {
        return buffer.Length >= 3 && buffer.Length <= 15;
    }

    private static int GetStatusCount(byte[] buffer)
    {
        if (buffer[0] < 0x41 && buffer[0] > 0x47) throw new ArgumentOutOfRangeException(nameof(buffer), Resources.Strings.InvalidHeader);
        return buffer[0] - 0x40;
    }

#pragma warning disable CA1819 // Properties should not return arrays
    public AccessoryDecoderInfo[] Changed { get; }
#pragma warning restore CA1819 // Properties should not return arrays
}
