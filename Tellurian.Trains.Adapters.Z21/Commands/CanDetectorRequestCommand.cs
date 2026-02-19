namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// LAN_CAN_DETECTOR (0xC4) - Request CAN detector state.
/// </summary>
public class CanDetectorRequestCommand : Command
{
    private readonly ushort _networkId;

    /// <summary>
    /// Creates a CAN detector request command.
    /// </summary>
    /// <param name="networkId">Network ID of the detector module. Use 0xD000 to query all detectors.</param>
    public CanDetectorRequestCommand(ushort networkId = 0xD000)
    {
        _networkId = networkId;
    }

    internal override Frame ToFrame()
    {
        var data = new byte[3];
        data[0] = 0x00; // Type = request
        data[1] = (byte)(_networkId & 0xFF);
        data[2] = (byte)((_networkId >> 8) & 0xFF);
        return new Frame(FrameHeader.CanDetector, data);
    }
}
