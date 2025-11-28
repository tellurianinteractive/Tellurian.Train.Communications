using System.Globalization;
using Tellurian.Trains.Interfaces.Decoder;

namespace Tellurian.Trains.Protocols.LocoNet.Programming;

/// <summary>
/// Represents the result of a programming operation from slot 124 response.
/// Parses OPC_SL_RD_DATA (slot 124) to extract programming status and CV value.
/// </summary>
public sealed class ProgrammingResult
{
    private ProgrammingResult(
        ProgrammingStatus status,
        CV cv,
        byte[] rawSlotData)
    {
        Status = status;
        CV = cv;
        RawSlotData = rawSlotData;
    }

    /// <summary>
    /// Programming status flags indicating success or error.
    /// </summary>
    public ProgrammingStatus Status { get; }

    /// <summary>
    /// CV number that was programmed (1-1024).
    /// </summary>
    public CV CV { get; }

    /// <summary>
    /// True if programming succeeded (no errors).
    /// </summary>
    public bool IsSuccess => Status == ProgrammingStatus.Success;

    /// <summary>
    /// True if programming failed (any error flags set).
    /// </summary>
    public bool IsFailure => Status != ProgrammingStatus.Success;

    /// <summary>
    /// Human-readable status message.
    /// </summary>
    public string StatusMessage => Status.GetMessage();

    /// <summary>
    /// Raw 14-byte slot data from response.
    /// </summary>
    public byte[] RawSlotData { get; }

    /// <summary>
    /// Parses a slot 124 response into a ProgrammingResult.
    /// </summary>
    /// <param name="slotData">14-byte slot data (OPC_SL_RD_DATA with slot 124)</param>
    /// <returns>Parsed programming result</returns>
    /// <exception cref="ArgumentException">If data is invalid or not slot 124</exception>
    public static ProgrammingResult FromSlotData(byte[] slotData)
    {
        if (slotData is null || slotData.Length != 14)
            throw new ArgumentException("Slot data must be exactly 14 bytes", nameof(slotData));

        byte opcode = slotData[0];
        if (opcode != 0xE7 && opcode != 0xEF)
            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture,
                    "Invalid opcode 0x{0:X2}. Expected 0xE7 or 0xEF", opcode),
                nameof(slotData));

        byte slot = slotData[2];
        if (slot != 0x7C)
            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture,
                    "Not a programming slot. Expected slot 124 (0x7C), got 0x{0:X2}", slot),
                nameof(slotData));

        // Byte 4 is PSTAT (programming status) in response
        byte pstat = slotData[4];
        var status = (ProgrammingStatus)(pstat & 0x0F);

        // Bytes 8, 9, 10 contain CVH, CVL, DATA7
        byte cvh = slotData[8];
        byte cvl = slotData[9];
        byte data7 = slotData[10];

        // Decode CV number and value
        var cv = CV.DecodeFromBytes(cvh, cvl, data7);

        return new ProgrammingResult(status, cv, slotData);
    }

    /// <summary>
    /// Parses a programming result from a SlotData structure.
    /// </summary>
    /// <param name="slotData">Parsed slot data</param>
    /// <returns>Programming result if slot 124, otherwise null</returns>
    public static ProgrammingResult? FromSlotData(SlotData slotData)
    {
        ArgumentNullException.ThrowIfNull(slotData);

        if (slotData.SlotNumber != 0x7C)
            return null;

        // Convert SlotData back to bytes and parse
        byte[] bytes = slotData.ToBytes(0xE7);
        byte[] fullMessage = new byte[14];
        Array.Copy(bytes, 0, fullMessage, 0, 13);
        fullMessage[13] = Message.Checksum(fullMessage);

        return FromSlotData(fullMessage);
    }

    public override string ToString()
    {
        if (IsSuccess)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0} (Success)",
                CV);
        }
        else
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0}: {1}",
                CV, StatusMessage);
        }
    }
}
