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

/// <summary>
/// Programming operation type (read or write).
/// </summary>
public enum ProgrammingOperation
{
    /// <summary>
    /// Read CV value from decoder.
    /// </summary>
    Read,

    /// <summary>
    /// Write CV value to decoder.
    /// </summary>
    Write
}

/// <summary>
/// Programming status flags from PSTAT byte in response.
/// </summary>
[Flags]
public enum ProgrammingStatus : byte
{
    /// <summary>
    /// No errors - programming succeeded.
    /// </summary>
    Success = 0,

    /// <summary>
    /// No decoder detected on programming track (bit 0).
    /// </summary>
    NoDecoder = 0b0001,

    /// <summary>
    /// Write acknowledge fail - decoder didn't acknowledge write (bit 1).
    /// </summary>
    WriteAckFail = 0b0010,

    /// <summary>
    /// Read compare acknowledge fail - no read acknowledgment (bit 2).
    /// </summary>
    ReadAckFail = 0b0100,

    /// <summary>
    /// User aborted operation (bit 3).
    /// </summary>
    UserAborted = 0b1000
}

/// <summary>
/// Helper methods for building PCMD byte and encoding CV/data values.
/// </summary>
public static class ProgrammingHelper
{
    /// <summary>
    /// Builds the PCMD (programming command) byte from programming parameters.
    /// </summary>
    /// <param name="mode">Programming mode</param>
    /// <param name="operation">Read or Write</param>
    /// <returns>PCMD byte value</returns>
    public static byte BuildPcmd(ProgrammingMode mode, ProgrammingOperation operation)
    {
        byte pcmd = 0;

        // Bit 6: Write/Read (1=Write, 0=Read)
        if (operation == ProgrammingOperation.Write)
            pcmd |= 0x40;

        // Bit 5: Byte mode (1=Byte operation, 0=Bit operation)
        // Bit 4: TY1
        // Bit 3: TY0
        // Bit 2: Ops mode (1=Operations mode, 0=Service mode)

        pcmd |= mode switch
        {
            ProgrammingMode.PagedModeService => 0b00100000, // byte=1, ops=0, TY=00
            ProgrammingMode.DirectModeByteService => 0b00101000, // byte=1, ops=0, TY=01
            ProgrammingMode.DirectModeBitService => 0b00001000, // byte=0, ops=0, TY=01
            ProgrammingMode.RegisterModeService => 0b00110000, // byte=x, ops=0, TY=10
            ProgrammingMode.OperationsModeByte => 0b00100100, // byte=1, ops=1, TY=00
            ProgrammingMode.OperationsModeByteWithFeedback => 0b00101100, // byte=1, ops=1, TY=01
            ProgrammingMode.OperationsModeBit => 0b00000100, // byte=0, ops=1, TY=00
            ProgrammingMode.OperationsModeBitWithFeedback => 0b00001100, // byte=0, ops=1, TY=01
            _ => throw new ArgumentException("Invalid programming mode", nameof(mode))
        };

        return pcmd;
    }

    /// <summary>
    /// Encodes a CV number (1-1024) into CVH and CVL bytes.
    /// </summary>
    /// <param name="cvNumber">CV number (1-1024)</param>
    /// <param name="dataValue">Data value (0-255) - bit 7 goes into CVH bit 1</param>
    /// <returns>Tuple of (cvh, cvl, data7)</returns>
    public static (byte cvh, byte cvl, byte data7) EncodeCvAndData(int cvNumber, byte dataValue)
    {
        if (cvNumber < 1 || cvNumber > 1024)
            throw new ArgumentOutOfRangeException(nameof(cvNumber), "CV number must be 1-1024");

        // CV numbers are 1-indexed, but we transmit 0-indexed
        int cvIndex = cvNumber - 1;

        // CVL: bits 6-0 of CV index
        byte cvl = (byte)(cvIndex & 0x7F);

        // CVH: bit 0 = CV bit 7, bit 1 = data bit 7
        byte cvh = (byte)((cvIndex >> 7) & 0x01);

        // Data bit 7 goes into CVH bit 1
        if ((dataValue & 0x80) != 0)
            cvh |= 0x02;

        // DATA7: bits 6-0 of data value
        byte data7 = (byte)(dataValue & 0x7F);

        return (cvh, cvl, data7);
    }

    /// <summary>
    /// Decodes CVH and CVL bytes back to CV number and data value.
    /// </summary>
    /// <param name="cvh">CVH byte</param>
    /// <param name="cvl">CVL byte</param>
    /// <param name="data7">DATA7 byte</param>
    /// <returns>Tuple of (cvNumber, dataValue)</returns>
    public static (int cvNumber, byte dataValue) DecodeCvAndData(byte cvh, byte cvl, byte data7)
    {
        // Reconstruct CV index from CVL (bits 6-0) and CVH bit 0 (bit 7)
        int cvIndex = cvl | ((cvh & 0x01) << 7);

        // CV numbers are 1-indexed
        int cvNumber = cvIndex + 1;

        // Reconstruct data value from DATA7 (bits 6-0) and CVH bit 1 (bit 7)
        byte dataValue = (byte)(data7 | ((cvh & 0x02) << 6));

        return (cvNumber, dataValue);
    }

    /// <summary>
    /// Checks if programming status indicates success.
    /// </summary>
    public static bool IsSuccess(ProgrammingStatus status)
    {
        return status == ProgrammingStatus.Success;
    }

    /// <summary>
    /// Gets a human-readable error message for programming status.
    /// </summary>
    public static string GetStatusMessage(ProgrammingStatus status)
    {
        if (status == ProgrammingStatus.Success)
            return "Programming succeeded";

        var messages = new List<string>();

        if ((status & ProgrammingStatus.NoDecoder) != 0)
            messages.Add("No decoder detected on programming track");

        if ((status & ProgrammingStatus.WriteAckFail) != 0)
            messages.Add("Write acknowledge failed");

        if ((status & ProgrammingStatus.ReadAckFail) != 0)
            messages.Add("Read acknowledge failed");

        if ((status & ProgrammingStatus.UserAborted) != 0)
            messages.Add("User aborted operation");

        return string.Join("; ", messages);
    }
}
