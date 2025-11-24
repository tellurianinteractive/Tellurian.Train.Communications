using System.Runtime.CompilerServices;
using Tellurian.Trains.Interfaces.Extensions;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

[assembly: InternalsVisibleTo("Tellurian.Trains.Protocols.LocoNet.Tests")]

namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// Base class for all LocoNet commands and notifications.
/// </summary>
public class Message
{
    /// <summary>
    /// Creates the appropriate type of <see cref="Message"/> from binary data.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static Message CreateMessage(byte[] buffer)
    {
        if (buffer is null || buffer.Length == 0)
            throw new ArgumentNullException(nameof(buffer));
        return (buffer[0]) switch
        {
            MasterBusyNotification.OperationCode => new MasterBusyNotification(),
            LongAcknowledge.OperationCode => new LongAcknowledge(buffer),
            _ => new UnsupportedNotification(buffer),
        };
    }

    public static byte Checksum(byte[] data)
    {
        if (data is null || data.Length == 0) return 0;
        var check = data[0];
        for (var i = 1; i < data.Length - 1; i++)
        {
            check ^= data[i];
        }
        return (byte)(~check);
    }

    public static byte[] AppendChecksum(byte[] dataWithoutChecksum)
    {
        if (dataWithoutChecksum is null) return [];
        var length = dataWithoutChecksum.Length;
        var result = new byte[length + 1];
        Array.Copy(dataWithoutChecksum, 0, result, 0, length);
        result[length] = Checksum(result);
        return result;
    }

    public static byte[] AppendChecksum(byte singleByteData)
    {
        return AppendChecksum([singleByteData]);
    }

    public override string ToString()
    {
        return GetType().Name;
    }
}

public static class MessageExtensions
{
    extension(Message message)
    {
        public Interfaces.Notification[] Map =>
                message is null ?
                [] :
                MapDefaults.CreateUnmapped(message.ToString()); // We do not support LocoNet notifications yet.
    }

}
