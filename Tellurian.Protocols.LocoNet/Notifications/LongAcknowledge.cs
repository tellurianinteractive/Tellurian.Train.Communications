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
            // Switch commands (OPC_SW_REQ 0xB0, OPC_SW_ACK 0xBD)
            case 0xB0: // OPC_SW_REQ
            case SwitchAcknowledgeCommand.OperationCode: // OPC_SW_ACK 0xBD
                if (me.ResponseCode == 0x7F) { me.Message = Resources.Strings.Accepted; return true; }
                if (me.ResponseCode == 0x00) { me.Message = Resources.Strings.FifoIsFull; return false; }
                break;

            // Move slots command
            case MoveSlotCommand.OperationCode:
                if (me.ResponseCode == 0x00) { me.Message = Resources.Strings.IllegalMove; return false; }
                break;

            // Loco address request
            case GetLocoAddressCommand.OperationCode:
                if (me.ResponseCode == 0x00) { me.Message = "No free slots available"; return false; }
                break;

            // Link/Unlink slots
            case 0xB9: // OPC_LINK_SLOTS
                if (me.ResponseCode == 0x00) { me.Message = "Invalid link operation"; return false; }
                break;
            case 0xB8: // OPC_UNLINK_SLOTS
                if (me.ResponseCode == 0x00) { me.Message = "Invalid unlink operation"; return false; }
                break;

            // Programming operations (slot 124)
            case 0x7F: // Programming responses
                if (me.ResponseCode == 0x7F) { me.Message = "Function not implemented"; return false; }
                if (me.ResponseCode == 0x00) { me.Message = "Programmer busy"; return false; }
                if (me.ResponseCode == 0x01) { me.Message = "Accepted, will send response"; return true; }
                if (me.ResponseCode == 0x40) { me.Message = "Accepted, blind operation"; return true; }
                break;

            default:
                me.Message = Resources.Strings.Undecided;
                break;
        }
        return null;
    }
}
