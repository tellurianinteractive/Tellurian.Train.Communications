using Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

public class LongAcknowledge : Notification
{
    public const byte OperationCode = 0xB4;
    public byte ForOperationCode { get; }
    public byte ResponseCode { get; }
    private readonly Lazy<bool?> Outcome;
    internal LongAcknowledge(byte[] data)
    {
        ValidateData(OperationCode, data);
        ForOperationCode = (byte)(data[1] | 0x80);
        ResponseCode = data[2];
        Outcome = new Lazy<bool?>(() => GetOutcome(this));
        Message = string.Empty;
    }
    public bool IsSuccess => Outcome.Value ?? false;
    public bool IsFailure => !Outcome.Value ?? false;
    public bool IsUndecided => !Outcome.Value.HasValue;
    public string Message { get; private set; }

    private static bool? GetOutcome(LongAcknowledge me)
    {
        switch (me.ForOperationCode)
        {
            case SetTurnoutCommand.OperationCode:
                if (me.ResponseCode == 0x7F) { me.Message = Resources.Strings.Accepted; return true; }
                if (me.ResponseCode == 0x00) { me.Message = Resources.Strings.FifoIsFull; return false; }
                break;
            case MoveSlotCommand.OperationCode:
                me.Message = Resources.Strings.IllegalMove;
                return false;
            default:
                me.Message = Resources.Strings.Undecided;
                break;
        }
        return null;
    }
}
