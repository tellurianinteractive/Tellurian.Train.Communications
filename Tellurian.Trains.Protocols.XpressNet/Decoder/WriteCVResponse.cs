using Tellurian.Trains.Interfaces.Extensions;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Decoder;

public abstract class WriteCVResponse : Notification
{
    protected WriteCVResponse(byte header, byte data) : base(header, data) { }
    protected WriteCVResponse(byte header, byte[] data) : base(header, data) { }
}

public sealed class WriteCVShortCircuitResponse : WriteCVResponse
{
    internal WriteCVShortCircuitResponse() : base(0x61, 0x12) { }
}

public sealed class WriteCVTimeoutResponse : WriteCVResponse
{
    internal WriteCVTimeoutResponse() : base(0x61, 0x13) { }
}

public sealed class CVOkResponse : WriteCVResponse
{
    internal CVOkResponse(byte[] data) : base(0x61, data) { }
    public CvAddress CvAddress => new CvAddress(Data, 2);
    public byte Value => Data[4];
}
