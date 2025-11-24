namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Sent from Z21 as response on <see cref="GetTurnoutAddressMode"/>.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 3.3
/// </remarks>
public sealed class TurnoutAddressModeNotification : AddressModeNotification
{
    internal TurnoutAddressModeNotification(Frame frame) : base(frame) { }
}
