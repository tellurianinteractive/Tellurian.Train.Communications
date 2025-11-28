using System.Globalization;

namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

/// <summary>
/// OPC_INPUT_REP (0xB2) - General sensor input report.
/// Reports sensor state changes from occupancy detectors, block detectors, buttons, etc.
/// Supports 11-bit addressing (0-2047), expandable to 4096 with I bit.
/// </summary>
public sealed class SensorInputNotification : Notification
{
    public const byte OperationCode = 0xB2;

    internal SensorInputNotification(byte[] data)
    {
        if (data is null || data.Length != 4)
            throw new ArgumentException("Sensor input report must be exactly 4 bytes", nameof(data));

        ValidateData(OperationCode, data);

        byte in1 = data[1];
        byte in2 = data[2];

        // Extract address from in1 (bits A6-A0) and in2 (bits A10-A7)
        Address = (ushort)(in1 | ((in2 & 0x0F) << 7));

        // Bit 6: X (must be 1 for valid message)
        bool xBit = (in2 & 0x40) != 0;
        if (!xBit)
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture,
                    "Invalid sensor input message: X bit (bit 6) must be 1, got 0x{0:X2}",
                    in2));
        }

        // Bit 5: I (0=DS54 aux inputs, 1=switch inputs in 4K sensor space)
        IsSwitchInput = (in2 & 0x20) != 0;

        // Bit 4: L (0=LOW/0V, 1=HIGH/>6V)
        IsHigh = (in2 & 0x10) != 0;
    }

    /// <summary>
    /// Sensor address (0-2047, or up to 4095 with switch input bit).
    /// </summary>
    public ushort Address { get; }

    /// <summary>
    /// True if this is a switch input in the 4K sensor space, false if DS54 aux input.
    /// </summary>
    public bool IsSwitchInput { get; }

    /// <summary>
    /// True if sensor input is HIGH (>6V), false if LOW (0V).
    /// For occupancy detectors: HIGH typically means occupied, LOW means clear.
    /// </summary>
    public bool IsHigh { get; }

    /// <summary>
    /// Inverse of IsHigh. True if sensor is LOW (0V).
    /// </summary>
    public bool IsLow => !IsHigh;

    /// <summary>
    /// For occupancy/block detection: true if occupied (HIGH).
    /// </summary>
    public bool IsOccupied => IsHigh;

    /// <summary>
    /// For occupancy/block detection: true if clear (LOW).
    /// </summary>
    public bool IsClear => IsLow;

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture,
            "Sensor {0}: {1} ({2})",
            Address,
            IsHigh ? "HIGH" : "LOW",
            IsSwitchInput ? "Switch" : "Aux");
    }
}
