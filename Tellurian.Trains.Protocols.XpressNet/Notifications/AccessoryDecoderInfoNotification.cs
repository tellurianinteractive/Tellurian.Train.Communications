namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Accessory Decoder information response (spec section 3.11).
/// This response is sent by the command station in response to an AccessoryInfoRequestCommand.
/// </summary>
/// <remarks>
/// Format: Header=0x42, Data=[Address, ITTNZZZZ]
/// - Address: Group address (turnout address / 4)
/// - I: Incomplete flag (1 = turnout has not reached end position)
/// - TT: Type code (00=no feedback, 01=with feedback, 10=feedback module)
/// - N: Nibble (0=lower, 1=upper)
/// - ZZZZ: Status flags for the two turnouts/4 inputs in the nibble
/// </remarks>
public sealed class AccessoryDecoderInfoNotification : Notification
{
    internal AccessoryDecoderInfoNotification(byte[] buffer) : base(0x42, GetData(buffer)) { }

    private static new byte[] GetData(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Length < 3) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer must contain at least 3 bytes");
        return [buffer[1], buffer[2]];
    }

    /// <summary>
    /// Gets the group address (turnout address / 4).
    /// </summary>
    public byte GroupAddress => Data[0];

    /// <summary>
    /// Gets the raw info byte containing type, nibble, and status flags.
    /// </summary>
    public byte InfoByte => Data[1];

    /// <summary>
    /// Gets whether the switching command is incomplete (turnout has not reached end position).
    /// Always false for feedback modules.
    /// </summary>
    public bool IsIncomplete => (InfoByte & 0x80) != 0;

    /// <summary>
    /// Gets the decoder type.
    /// </summary>
    public AccessoryDecoderType DecoderType => (AccessoryDecoderType)((InfoByte >> 5) & 0x03);

    /// <summary>
    /// Gets whether this response is for the upper nibble (turnouts 2-3 or inputs 4-7).
    /// </summary>
    public bool IsUpperNibble => (InfoByte & 0x10) != 0;

    /// <summary>
    /// Gets the status flags (Z3-Z0) for the turnouts or feedback inputs.
    /// </summary>
    public byte StatusFlags => (byte)(InfoByte & 0x0F);

    /// <summary>
    /// Gets the status of the first turnout in the nibble (Z1, Z0).
    /// </summary>
    public TurnoutStatus FirstTurnoutStatus => (TurnoutStatus)(StatusFlags & 0x03);

    /// <summary>
    /// Gets the status of the second turnout in the nibble (Z3, Z2).
    /// </summary>
    public TurnoutStatus SecondTurnoutStatus => (TurnoutStatus)((StatusFlags >> 2) & 0x03);

    /// <summary>
    /// Gets the base turnout address (1-indexed) for the first turnout in this response.
    /// </summary>
    public int FirstTurnoutAddress => (GroupAddress * 4) + (IsUpperNibble ? 2 : 0) + 1;

    /// <summary>
    /// Gets the base turnout address (1-indexed) for the second turnout in this response.
    /// </summary>
    public int SecondTurnoutAddress => FirstTurnoutAddress + 1;
}

/// <summary>
/// Type of accessory decoder.
/// </summary>
public enum AccessoryDecoderType : byte
{
    /// <summary>
    /// Accessory decoder without feedback capability.
    /// </summary>
    WithoutFeedback = 0,

    /// <summary>
    /// Accessory decoder with feedback capability (e.g., LS100/110).
    /// </summary>
    WithFeedback = 1,

    /// <summary>
    /// Feedback module (e.g., LR100/101).
    /// </summary>
    FeedbackModule = 2,

    /// <summary>
    /// Reserved for future use.
    /// </summary>
    Reserved = 3
}

/// <summary>
/// Status of a turnout from accessory decoder feedback.
/// </summary>
public enum TurnoutStatus : byte
{
    /// <summary>
    /// Turnout has not been controlled during this operating session.
    /// </summary>
    NotControlled = 0,

    /// <summary>
    /// Last command was "0" (turnout "left"/diverging).
    /// </summary>
    Diverging = 1,

    /// <summary>
    /// Last command was "1" (turnout "right"/straight).
    /// </summary>
    Straight = 2,

    /// <summary>
    /// Invalid - both end switches indicate active (error condition).
    /// </summary>
    Invalid = 3
}
