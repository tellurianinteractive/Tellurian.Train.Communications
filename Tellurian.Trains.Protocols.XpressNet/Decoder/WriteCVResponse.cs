using Tellurian.Trains.Communications.Interfaces.Decoder;
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

    /// <summary>
    /// Gets the CV number and value.
    /// </summary>
    public CV CV => new(DecodeCvNumber(), Data[4]);

    /// <summary>
    /// Decodes CV number from wire format (MSB at offset 3, LSB at offset 2).
    /// Wire value 0-1023 maps to CV number 1-1024.
    /// </summary>
    private int DecodeCvNumber() => ((Data[3] << 8) + Data[2]) + 1;
}
