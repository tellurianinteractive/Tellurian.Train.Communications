using Tellurian.Trains.Interfaces.Extensions;

namespace Tellurian.Trains.Interfaces.Decoder;

public sealed class DecoderResponse : Notification
{
    public static DecoderResponse Success(CvAddress cv, byte value) => new DecoderResponse(cv, value, true, string.Empty);
    public static DecoderResponse Timeout() => new DecoderResponse(0.CV(), 0, false, "Timeout");
    public static DecoderResponse Shortcircuit() => new DecoderResponse(0.CV(), 0, false, "Short circuit");
    private DecoderResponse(CvAddress cv, byte value, bool isSuccess, string errorReason )
    {
        CV = cv;
        Value = value;
        IsSuccess = isSuccess;
        ErrorReason = errorReason;
    }

    public CvAddress CV { get; }
    public byte Value { get; }
    public bool IsSuccess { get; }
    public string ErrorReason { get; }
}
