using System.Diagnostics.CodeAnalysis;

namespace Tellurian.Trains.Protocols.LocoNet.Programming;

public readonly struct CV
{
    public int Number { get; init; }
    public byte Value { get; init; }
    public override bool Equals([NotNullWhen(true)] object? obj) => base.Equals(obj);
    public override int GetHashCode() => HashCode.Combine(Number, Value);
    override public string ToString() => $"CV{Number}={Value}";

    public static bool operator ==(CV left, CV right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CV left, CV right)
    {
        return !(left == right);
    }
}


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
    public static byte BuildProgrammingCommandByte(ProgrammingMode mode, ProgrammingOperation operation)
    {
        byte programmingCommandByte = 0;

        // Bit 6: Write/Read (1=Write, 0=Read)
        if (operation == ProgrammingOperation.Write)
            programmingCommandByte |= 0x40;

        // Bit 5: Byte mode (1=Byte operation, 0=Bit operation)
        // Bit 4: TY1
        // Bit 3: TY0
        // Bit 2: Ops mode (1=Operations mode, 0=Service mode)

        programmingCommandByte |= mode switch
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

        return programmingCommandByte;
    }

    extension(CV cv)
    {
        /// <summary>
        /// Encodes a CV number (1-1024) into CVH and CVL bytes.
        /// </summary>
        /// <param name="cvNumber">CV number (1-1024)</param>
        /// <param name="dataValue">Data value (0-255) - bit 7 goes into CVH bit 1</param>
        /// <returns>Tuple of (cvh, cvl, data7)</returns>
        public  (byte cvh, byte cvl, byte data7) EncodeCvAndData()
        {
            if (cv.Number < 1 || cv.Number > 1024)
                throw new ArgumentOutOfRangeException(nameof(cv.Number), "CV number must be 1-1024");

            // CV numbers are 1-indexed, but we transmit 0-indexed
            int cvIndex = cv.Number - 1;

            // CVL: bits 6-0 of CV index
            byte cvl = (byte)(cvIndex & 0x7F);

            // CVH: bit 0 = CV bit 7, bit 1 = data bit 7
            byte cvh = (byte)((cvIndex >> 7) & 0x01);

            // Data bit 7 goes into CVH bit 1
            if ((cv.Value & 0x80) != 0)
                cvh |= 0x02;

            // DATA7: bits 6-0 of data value
            byte data7 = (byte)(cv.Value & 0x7F);

            return (cvh, cvl, data7);
        }

        /// <summary>
        /// Decodes CVH and CVL bytes back to CV number and data value.
        /// </summary>
        /// <param name="cvh">CVH byte</param>
        /// <param name="cvl">CVL byte</param>
        /// <param name="data7">DATA7 byte</param>
        /// <returns>Tuple of (cvNumber, dataValue)</returns>
        public static CV DecodeCvAndData(byte cvh, byte cvl, byte data7)
        {
            // Reconstruct CV index from CVL (bits 6-0) and CVH bit 0 (bit 7)
            int cvIndex = cvl | ((cvh & 0x01) << 7);

            // CV numbers are 1-indexed
            int cvNumber = cvIndex + 1;

            // Reconstruct data value from DATA7 (bits 6-0) and CVH bit 1 (bit 7)
            byte cvValue = (byte)(data7 | ((cvh & 0x02) << 6));

            return new() { Number = cvNumber, Value = cvValue };
        }
    }
}
