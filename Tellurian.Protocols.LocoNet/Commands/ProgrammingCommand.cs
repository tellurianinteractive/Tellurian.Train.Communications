using Tellurian.Trains.Protocols.LocoNet.Programming;

namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// Programming command using slot 124 (OPC_WR_SL_DATA with slot 0x7C).
/// Used for CV read/write operations on the programming track or operations mode (POM).
/// Response: OPC_LONG_ACK followed by OPC_SL_RD_DATA (slot 124) with result.
/// </summary>
public sealed class ProgrammingCommand : Command
{
    public const byte OperationCode = 0xEF;
    public const byte ProgrammingSlot = 0x7C; // Slot 124

    private ProgrammingCommand(
        ProgrammingMode mode,
        ProgrammingOperation operation,
        int cvNumber,
        byte cvValue,
        ushort locomotiveAddress,
        byte trackStatus)
    {
        Mode = mode;
        Operation = operation;
        CV = new() { Number=cvNumber, Value = cvValue};
        LocomotiveAddress = locomotiveAddress;
        TrackStatus = trackStatus;
    }

    /// <summary>
    /// Programming mode.
    /// </summary>
    public ProgrammingMode Mode { get; }

    /// <summary>
    /// Read or Write operation.
    /// </summary>
    public ProgrammingOperation Operation { get; }

    /// <summary>
    /// CV number to program (1-1024).
    /// </summary>
    public CV CV { get; }

    /// <summary>
    /// Locomotive address for operations mode (POM). Zero for service mode.
    /// </summary>
    public ushort LocomotiveAddress { get; }

    /// <summary>
    /// Track status byte (usually 0x03 for power on).
    /// </summary>
    public byte TrackStatus { get; }

    /// <summary>
    /// Creates a service mode CV read command (programming track).
    /// </summary>
    /// <param name="cvNumber">CV number (1-1024)</param>
    /// <param name="mode">Service mode type (default: Direct byte)</param>
    public static ProgrammingCommand ReadCvService(
        int cvNumber,
        ProgrammingMode mode = ProgrammingMode.DirectModeByteService)
    {
        if (mode == ProgrammingMode.OperationsModeByte ||
            mode == ProgrammingMode.OperationsModeByteWithFeedback ||
            mode == ProgrammingMode.OperationsModeBit ||
            mode == ProgrammingMode.OperationsModeBitWithFeedback)
            throw new ArgumentException("Use ReadCvOperations for operations mode", nameof(mode));

        return new ProgrammingCommand(
            mode,
            ProgrammingOperation.Read,
            cvNumber,
            0, // Data value ignored for reads
            0, // No loco address for service mode
            0x03); // Track power on
    }

    /// <summary>
    /// Creates a service mode CV write command (programming track).
    /// </summary>
    /// <param name="cvNumber">CV number (1-1024)</param>
    /// <param name="value">Value to write (0-255)</param>
    /// <param name="mode">Service mode type (default: Direct byte)</param>
    public static ProgrammingCommand WriteCvService(
        int cvNumber,
        byte value,
        ProgrammingMode mode = ProgrammingMode.DirectModeByteService)
    {
        if (mode == ProgrammingMode.OperationsModeByte ||
            mode == ProgrammingMode.OperationsModeByteWithFeedback ||
            mode == ProgrammingMode.OperationsModeBit ||
            mode == ProgrammingMode.OperationsModeBitWithFeedback)
            throw new ArgumentException("Use WriteCvOperations for operations mode", nameof(mode));

        return new ProgrammingCommand(
            mode,
            ProgrammingOperation.Write,
            cvNumber,
            value,
            0, // No loco address for service mode
            0x03); // Track power on
    }

    /// <summary>
    /// Creates an operations mode (POM) CV write command (main track).
    /// Note: POM reads are not supported by most decoders.
    /// </summary>
    /// <param name="locomotiveAddress">Locomotive address (1-9999)</param>
    /// <param name="cvNumber">CV number (1-1024)</param>
    /// <param name="value">Value to write (0-255)</param>
    /// <param name="withFeedback">True to request feedback (may not be supported)</param>
    public static ProgrammingCommand WriteCvOperations(
        ushort locomotiveAddress,
        int cvNumber,
        byte value,
        bool withFeedback = false)
    {
        if (locomotiveAddress < 1 || locomotiveAddress > 9999)
            throw new ArgumentOutOfRangeException(nameof(locomotiveAddress), "Address must be 1-9999");

        var mode = withFeedback
            ? ProgrammingMode.OperationsModeByteWithFeedback
            : ProgrammingMode.OperationsModeByte;

        return new ProgrammingCommand(
            mode,
            ProgrammingOperation.Write,
            cvNumber,
            value,
            locomotiveAddress,
            0x03); // Track power on
    }

    /// <summary>
    /// Creates an abort programming command.
    /// </summary>
    public static ProgrammingCommand Abort()
    {
        return new ProgrammingCommand(
            ProgrammingMode.DirectModeByteService,
            ProgrammingOperation.Read,
            1, // CV1 (dummy)
            0,
            0,
            0); // PCMD will be set to 0x00 to abort
    }

    /// <summary>
    /// Generates the 14-byte message: [0xEF, 0x0E, 0x7C, pcmd, 0, hopsa, lopsa, trk, cvh, cvl, data7, 0, 0, checksum].
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        byte[] data = new byte[13];

        data[0] = OperationCode; // 0xEF
        data[1] = 0x0E; // Byte count (14)
        data[2] = ProgrammingSlot; // Slot 124

        // Build PCMD byte
        byte programmingCommandByte = ProgrammingHelper.BuildProgrammingCommandByte(Mode, Operation);
        data[3] = programmingCommandByte;

        data[4] = 0x00; // Reserved

        // HOPSA/LOPSA: Locomotive address for ops mode, zero for service mode
        if (LocomotiveAddress > 0)
        {
            // Split address for operations mode
            data[5] = (byte)((LocomotiveAddress >> 7) & 0x7F); // HOPSA
            data[6] = (byte)(LocomotiveAddress & 0x7F); // LOPSA
        }
        else
        {
            data[5] = 0x00; // Service mode
            data[6] = 0x00;
        }

        data[7] = TrackStatus; // TRK status

        // Encode CV and data
        var (cvh, cvl, data7) = ProgrammingHelper.EncodeCvAndData(CV);
        data[8] = cvh; // CVH
        data[9] = cvl; // CVL
        data[10] = data7; // DATA7

        data[11] = 0x00; // Reserved
        data[12] = 0x00; // Reserved

        return AppendChecksum(data);
    }

    public override string ToString()
    {
        string operationStr = Operation == ProgrammingOperation.Read ? "Read" : "Write";
        string modeStr = Mode.ToString();
        string addressStr = LocomotiveAddress > 0 ? $" (Loco {LocomotiveAddress})" : " (Service)";

        return $"{operationStr} {CV} {modeStr}{addressStr}";
    }
}
