namespace Tellurian.Trains.Protocols.XpressNet.Commands;

public sealed class LocoDriveCommand : Command
{
    public LocoDriveCommand(LocoAddress address, LocoSpeed speed, LocoDirection direction) : base(0xE0, GetData(address, speed, direction)) { }

    private static byte[] GetData(LocoAddress address, LocoSpeed speed, LocoDirection direction) {
        var result = new byte[4];
        result[0] = speed.Code;
        Array.Copy(address.GetBytesAccordingToXpressNet(), 0, result, 1, 2);
        result[3] = (byte)(speed.Current + (128 * (byte)direction));
        return result;
    }
}
