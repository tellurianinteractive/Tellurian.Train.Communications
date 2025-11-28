using System.Globalization;

namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// Represents the complete data structure for a LocoNet slot (locomotive control record).
/// Corresponds to OPC_SL_RD_DATA (0xE7) and OPC_WR_SL_DATA (0xEF) 14-byte messages.
/// </summary>
public sealed class SlotData
{
    private const int ExpectedMessageLength = 14;

    /// <summary>
    /// Slot number (0-127). Special slots: 0=dispatch, 123=fast clock, 124=programming.
    /// </summary>
    public required byte SlotNumber { get; init; }

    /// <summary>
    /// Locomotive address (1-9999). Combines ADR (byte 4) and ADR2 (byte 9).
    /// </summary>
    public required ushort Address { get; init; }

    /// <summary>
    /// Speed value (0-127). 0=stop with momentum, 1=emergency stop, 2-127=speed steps.
    /// </summary>
    public required byte Speed { get; init; }

    /// <summary>
    /// Direction: true=forward, false=reverse.
    /// </summary>
    public required bool Direction { get; init; }

    /// <summary>
    /// Slot status (FREE, COMMON, IDLE, IN_USE).
    /// </summary>
    public required SlotStatus Status { get; init; }

    /// <summary>
    /// Consist status (not in consist, sub-member, top, mid-consist).
    /// </summary>
    public required ConsistStatus Consist { get; init; }

    /// <summary>
    /// Decoder type/speed step mode.
    /// </summary>
    public required DecoderType DecoderType { get; init; }

    /// <summary>
    /// Global track status flags.
    /// </summary>
    public required TrackStatus TrackStatus { get; init; }

    /// <summary>
    /// Function F0 state (typically headlight).
    /// </summary>
    public required bool F0 { get; init; }

    /// <summary>
    /// Function F1 state.
    /// </summary>
    public required bool F1 { get; init; }

    /// <summary>
    /// Function F2 state.
    /// </summary>
    public required bool F2 { get; init; }

    /// <summary>
    /// Function F3 state.
    /// </summary>
    public required bool F3 { get; init; }

    /// <summary>
    /// Function F4 state.
    /// </summary>
    public required bool F4 { get; init; }

    /// <summary>
    /// Function F5 state.
    /// </summary>
    public required bool F5 { get; init; }

    /// <summary>
    /// Function F6 state.
    /// </summary>
    public required bool F6 { get; init; }

    /// <summary>
    /// Function F7 state.
    /// </summary>
    public required bool F7 { get; init; }

    /// <summary>
    /// Function F8 state.
    /// </summary>
    public required bool F8 { get; init; }

    /// <summary>
    /// Device ID that is controlling this slot (14-bit value from ID1 and ID2).
    /// </summary>
    public required ushort DeviceId { get; init; }

    /// <summary>
    /// Status byte 2 (reserved/extended status).
    /// </summary>
    public required byte Status2 { get; init; }

    /// <summary>
    /// Parses a 14-byte LocoNet slot message into a SlotData structure.
    /// </summary>
    /// <param name="data">14-byte array containing slot message (including opcode and checksum)</param>
    /// <returns>Parsed SlotData structure</returns>
    /// <exception cref="ArgumentNullException">If data is null</exception>
    /// <exception cref="ArgumentException">If data length is not 14 bytes or opcode is invalid</exception>
    public static SlotData FromBytes(byte[] data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        if (data.Length != ExpectedMessageLength)
            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture,
                    "Expected {0} bytes but received {1} bytes",
                    ExpectedMessageLength, data.Length),
                nameof(data));

        byte opcode = data[0];
        if (opcode != 0xE7 && opcode != 0xEF)
            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture,
                    "Invalid opcode 0x{0:X2}. Expected 0xE7 (OPC_SL_RD_DATA) or 0xEF (OPC_WR_SL_DATA)",
                    opcode),
                nameof(data));

        byte byteCount = data[1];
        if (byteCount != 0x0E)
            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture,
                    "Invalid byte count 0x{0:X2}. Expected 0x0E (14 bytes)",
                    byteCount),
                nameof(data));

        // Extract fields from message
        byte slotNumber = data[2];
        byte stat1 = data[3];
        byte adrLo = data[4];
        byte spd = data[5];
        byte dirf = data[6];
        byte trk = data[7];
        byte ss2 = data[8];
        byte adrHi = data[9];
        byte snd = data[10];
        byte id1 = data[11];
        byte id2 = data[12];

        // Parse STAT1 byte
        SlotStatus status = (SlotStatus)((stat1 >> 4) & 0b11);
        ConsistStatus consist = (ConsistStatus)(((stat1 >> 3) & 0b1000) | ((stat1 >> 3) & 0b0001));
        DecoderType decoderType = (DecoderType)(stat1 & 0b111);

        // Parse address (7-bit low + 7-bit high)
        ushort address = (ushort)(adrLo | (adrHi << 7));

        // Parse direction and functions from DIRF
        bool direction = (dirf & 0x20) != 0;
        bool f0 = (dirf & 0x10) != 0;
        bool f4 = (dirf & 0x08) != 0;
        bool f3 = (dirf & 0x04) != 0;
        bool f2 = (dirf & 0x02) != 0;
        bool f1 = (dirf & 0x01) != 0;

        // Parse functions from SND
        bool f8 = (snd & 0x08) != 0;
        bool f7 = (snd & 0x04) != 0;
        bool f6 = (snd & 0x02) != 0;
        bool f5 = (snd & 0x01) != 0;

        // Parse device ID (7-bit low + 7-bit high)
        ushort deviceId = (ushort)(id1 | (id2 << 7));

        return new SlotData
        {
            SlotNumber = slotNumber,
            Address = address,
            Speed = spd,
            Direction = direction,
            Status = status,
            Consist = consist,
            DecoderType = decoderType,
            TrackStatus = (TrackStatus)trk,
            F0 = f0,
            F1 = f1,
            F2 = f2,
            F3 = f3,
            F4 = f4,
            F5 = f5,
            F6 = f6,
            F7 = f7,
            F8 = f8,
            DeviceId = deviceId,
            Status2 = ss2
        };
    }

    /// <summary>
    /// Converts this SlotData structure to a 14-byte LocoNet message (without checksum).
    /// </summary>
    /// <param name="opcode">Opcode to use: 0xE7 for read, 0xEF for write</param>
    /// <returns>13-byte array (caller must append checksum)</returns>
    public byte[] ToBytes(byte opcode = 0xEF)
    {
        if (opcode != 0xE7 && opcode != 0xEF)
            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture,
                    "Invalid opcode 0x{0:X2}. Must be 0xE7 or 0xEF",
                    opcode),
                nameof(opcode));

        byte[] data = new byte[13];

        data[0] = opcode;
        data[1] = 0x0E;  // Byte count
        data[2] = SlotNumber;

        // Build STAT1 byte
        byte stat1 = (byte)(
            ((byte)Status << 4) |
            ((byte)Consist & 0b1000) |  // CONUP bit
            ((byte)Consist & 0b0001) << 3 |  // CONDN bit
            (byte)DecoderType
        );
        data[3] = stat1;

        // Split address
        data[4] = (byte)(Address & 0x7F);        // ADR low
        data[9] = (byte)((Address >> 7) & 0x7F); // ADR high

        data[5] = Speed;

        // Build DIRF byte
        byte dirf = (byte)(
            (Direction ? 0x20 : 0) |
            (F0 ? 0x10 : 0) |
            (F4 ? 0x08 : 0) |
            (F3 ? 0x04 : 0) |
            (F2 ? 0x02 : 0) |
            (F1 ? 0x01 : 0)
        );
        data[6] = dirf;

        data[7] = (byte)TrackStatus;
        data[8] = Status2;

        // Build SND byte
        byte snd = (byte)(
            (F8 ? 0x08 : 0) |
            (F7 ? 0x04 : 0) |
            (F6 ? 0x02 : 0) |
            (F5 ? 0x01 : 0)
        );
        data[10] = snd;

        // Split device ID
        data[11] = (byte)(DeviceId & 0x7F);        // ID1 low
        data[12] = (byte)((DeviceId >> 7) & 0x7F); // ID2 high

        return data;
    }

    /// <summary>
    /// Gets a formatted string representation of this slot data.
    /// </summary>
    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture,
            "Slot {0}: Addr={1}, Speed={2}, Dir={3}, Status={4}, F0={5}",
            SlotNumber, Address, Speed, Direction ? "FWD" : "REV", Status, F0);
    }
}
