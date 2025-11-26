namespace Tellurian.Trains.Interfaces.Decoder;

public sealed class DecoderResponse : Notification
{
    public static DecoderResponse Success(CV cv) => new(cv, true, string.Empty);
    public static DecoderResponse Timeout() => new(new CV(1, 0), false, "Timeout");
    public static DecoderResponse Shortcircuit() => new(new CV(1, 0), false, "Short circuit");
    private DecoderResponse(CV cv, bool isSuccess, string errorReason)
    {
        CV = cv;
        IsSuccess = isSuccess;
        ErrorReason = errorReason;
    }

    public CV CV { get; }
    public bool IsSuccess { get; }
    public string ErrorReason { get; }
}
