namespace Tellurian.Trains.Protocols.LocoNet.Programming;


/// <summary>
/// Programming modes for decoder CV operations.
/// </summary>
public enum ProgrammingMode
{
    /// <summary>
    /// Paged mode byte read/write on service track (PCMD: byte=1, ops=0, TY1=0, TY0=0).
    /// </summary>
    PagedModeService,

    /// <summary>
    /// Direct mode byte read/write on service track (PCMD: byte=1, ops=0, TY1=0, TY0=1).
    /// Most commonly used service mode.
    /// </summary>
    DirectModeByteService,

    /// <summary>
    /// Direct mode bit read/write on service track (PCMD: byte=0, ops=0, TY1=0, TY0=1).
    /// </summary>
    DirectModeBitService,

    /// <summary>
    /// Physical register byte read/write on service track (PCMD: byte=x, ops=0, TY1=1, TY0=0).
    /// </summary>
    RegisterModeService,

    /// <summary>
    /// Operations mode byte, no feedback (PCMD: byte=1, ops=1, TY1=0, TY0=0).
    /// Programming on Main (POM) - blind write.
    /// </summary>
    OperationsModeByte,

    /// <summary>
    /// Operations mode byte with feedback (PCMD: byte=1, ops=1, TY1=0, TY0=1).
    /// Programming on Main (POM) with acknowledgment.
    /// </summary>
    OperationsModeByteWithFeedback,

    /// <summary>
    /// Operations mode bit, no feedback (PCMD: byte=0, ops=1, TY1=0, TY0=0).
    /// </summary>
    OperationsModeBit,

    /// <summary>
    /// Operations mode bit with feedback (PCMD: byte=0, ops=1, TY1=0, TY0=1).
    /// </summary>
    OperationsModeBitWithFeedback
}
