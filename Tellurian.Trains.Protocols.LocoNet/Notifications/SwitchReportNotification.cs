using System.Globalization;

namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

/// <summary>
/// OPC_SW_REP (0xB1) - Switch sensor report.
/// Broadcast from DS54 stationary decoders reporting turnout feedback or output status.
/// Can represent either input feedback (sensors) or output status (which direction is active).
/// </summary>
public sealed class SwitchReportNotification : Notification
{
    public const byte OperationCode = 0xB1;

    internal SwitchReportNotification(byte[] data)
    {
        if (data is null || data.Length != 4)
            throw new ArgumentException("Switch report must be exactly 4 bytes", nameof(data));

        ValidateData(OperationCode, data);

        byte sn1 = data[1];
        byte sn2 = data[2];

        // Extract address from sn1 (bits A6-A0) and sn2 (bits A10-A7)
        Address = new AccessoryAddress((ushort)(sn1 | ((sn2 & 0x0F) << 7)));

        // Bit 6 determines interpretation
        IsInputFeedback = (sn2 & 0x40) != 0;

        if (IsInputFeedback)
        {
            // Input feedback interpretation
            // Bit 5: I (0=aux input, 1=switch input)
            IsSwitchInput = (sn2 & 0x20) != 0;

            // Bit 4: L (0=LOW/0V, 1=HIGH/>6V)
            IsInputHigh = (sn2 & 0x10) != 0;

            // Output status not applicable
            ClosedOutputOn = false;
            ThrownOutputOn = false;
        }
        else
        {
            // Output status interpretation
            // Bit 5: C (1=Closed output ON)
            ClosedOutputOn = (sn2 & 0x20) != 0;

            // Bit 4: T (1=Thrown output ON)
            ThrownOutputOn = (sn2 & 0x10) != 0;

            // Input feedback not applicable
            IsSwitchInput = false;
            IsInputHigh = false;
        }
    }

    /// <summary>
    /// The accessory address being reported (0-2047).
    /// </summary>
    public AccessoryAddress Address { get; }

    /// <summary>
    /// True if this is input feedback, false if output status.
    /// </summary>
    public bool IsInputFeedback { get; }

    /// <summary>
    /// True if this is output status, false if input feedback.
    /// </summary>
    public bool IsOutputStatus => !IsInputFeedback;

    // Input Feedback Properties (valid when IsInputFeedback = true)

    /// <summary>
    /// For input feedback: true if switch input, false if aux input.
    /// Only valid when IsInputFeedback is true.
    /// </summary>
    public bool IsSwitchInput { get; }

    /// <summary>
    /// For input feedback: true if input is HIGH (>6V), false if LOW (0V).
    /// Only valid when IsInputFeedback is true.
    /// </summary>
    public bool IsInputHigh { get; }

    // Output Status Properties (valid when IsOutputStatus = true)

    /// <summary>
    /// For output status: true if closed output is ON.
    /// Only valid when IsOutputStatus is true.
    /// </summary>
    public bool ClosedOutputOn { get; }

    /// <summary>
    /// For output status: true if thrown output is ON.
    /// Only valid when IsOutputStatus is true.
    /// </summary>
    public bool ThrownOutputOn { get; }

    /// <summary>
    /// For output status: the current direction based on which output is active.
    /// Returns null if both or neither outputs are on, or if this is input feedback.
    /// </summary>
    public AccessoryFunction? CurrentDirection
    {
        get
        {
            if (IsInputFeedback) return null;
            if (ClosedOutputOn && !ThrownOutputOn) return AccessoryFunction.ClosedOrGreen;
            if (ThrownOutputOn && !ClosedOutputOn) return AccessoryFunction.ThrownOrRed;
            return null; // Both on, both off, or indeterminate
        }
    }

    public override string ToString()
    {
        if (IsInputFeedback)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Switch {0}: Input {1} {2}",
                Address.Value,
                IsSwitchInput ? "Switch" : "Aux",
                IsInputHigh ? "HIGH" : "LOW");
        }
        else
        {
            string status = CurrentDirection switch
            {
                AccessoryFunction.ClosedOrGreen => "CLOSED",
                AccessoryFunction.ThrownOrRed => "THROWN",
                _ => "UNKNOWN"
            };

            return string.Format(CultureInfo.InvariantCulture,
                "Switch {0}: Output {1}",
                Address.Value,
                status);
        }
    }
}
