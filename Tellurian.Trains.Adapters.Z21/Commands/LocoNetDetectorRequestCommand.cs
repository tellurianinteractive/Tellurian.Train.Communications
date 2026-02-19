namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Request type for LAN_LOCONET_DETECTOR queries.
/// </summary>
public enum DetectorRequestType : byte
{
    /// <summary>Stationary Interrogate (SIC) for Digitrax/Blücher detectors.</summary>
    StationaryInterrogate = 0x80,
    /// <summary>Uhlenbrock detector request.</summary>
    Uhlenbrock = 0x81,
    /// <summary>LISSY detector request (Z21 FW 1.23+).</summary>
    Lissy = 0x82
}

/// <summary>
/// LAN_LOCONET_DETECTOR (0xA4) - Request detector state via LocoNet emulation.
/// </summary>
public class LocoNetDetectorRequestCommand : Command
{
    private readonly DetectorRequestType _type;
    private readonly ushort _reportAddress;

    public LocoNetDetectorRequestCommand(DetectorRequestType type, ushort reportAddress)
    {
        _type = type;
        _reportAddress = reportAddress;
    }

    internal override Frame ToFrame()
    {
        var data = new byte[3];
        data[0] = (byte)_type;
        data[1] = (byte)(_reportAddress & 0xFF);
        data[2] = (byte)((_reportAddress >> 8) & 0xFF);
        return new Frame(FrameHeader.LocoNetDetector, data);
    }
}
