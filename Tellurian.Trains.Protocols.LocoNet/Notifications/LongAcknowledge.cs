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
        var (outcome, message) = (me.ForOperationCode, me.ResponseCode) switch
        {
            // Accessory commands (OPC_SW_REQ 0xB0, OPC_SW_ACK 0xBD)
            (0xB0, 0x7F) or (AccessoryAcknowledgeCommand.OperationCode, 0x7F)
                => (true, Resources.Strings.Accepted),
            (0xB0, 0x00) or (AccessoryAcknowledgeCommand.OperationCode, 0x00)
                => (false, Resources.Strings.FifoIsFull),

            // Move slots command
            (MoveSlotCommand.OperationCode, 0x00)
                => (false, Resources.Strings.IllegalMove),

            // Loco address request
            (GetLocoAddressCommand.OperationCode, 0x00)
                => (false, "No free slots available"),

            // Link slots
            (0xB9, 0x00)
                => (false, "Invalid link operation"),

            // Unlink slots
            (0xB8, 0x00)
                => (false, "Invalid unlink operation"),

            // Programming operations (slot 124)
            (0x7F, 0x7F) => (false, "Function not implemented"),
            (0x7F, 0x00) => (false, "Programmer busy"),
            (0x7F, 0x01) => (true, "Accepted, will send response"),
            (0x7F, 0x40) => (true, "Accepted, blind operation"),

            // Default case
            _ => ((bool?)null, Resources.Strings.Undecided)
        };

        me.Message = message;
        return outcome;
    }
}
