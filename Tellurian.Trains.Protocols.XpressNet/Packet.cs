using System.Globalization;
using Tellurian.Trains.Protocols.XpressNet.Commands;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet;

public class Packet
{
    private readonly byte[] _Data;

    public Packet(Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        _Data = command.GetData();
    }

    public Packet(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var payload = data.Take(data.Length - 1).ToArray();
        var xor = GetXorChecksum(payload);
        if (data.Last() != xor) throw new ArgumentOutOfRangeException(nameof(data), xor.ToString("X", CultureInfo.InvariantCulture));
        _Data = data;
    }

    public byte[] GetBytes()
    {
        var result = new byte[_Data.Length + 1];
        Array.Copy(_Data, result, _Data.Length);
        result[_Data.Length] = GetXorChecksum(_Data);
        return result;
    }

    public Notification Notification => NotificationFactory.Create(_Data);

    static private byte GetXorChecksum(byte[] value)
    {
        byte result = value[0];
        foreach (var d in value.Skip(1))
        {
            result = (byte)(result ^ d);
        }
        return result;
    }
}

public static class CommandExtensions
{
    public static byte[] GetBytes(this Command command)
    {
        return new Packet(command).GetBytes();
    }

    public static Notification Create(this byte[] data)
    {
        var packet = new Packet(data);
        return packet.Notification;
    }
}
