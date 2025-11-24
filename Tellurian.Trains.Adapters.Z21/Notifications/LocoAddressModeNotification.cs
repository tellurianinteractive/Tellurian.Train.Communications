namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Sent from Z21 as a response to <see cref="GetLocoAddressMode"/>.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 3.1
/// </remarks>
public sealed class LocoAddressModeNotification : AddressModeNotification
{
    internal LocoAddressModeNotification(Frame frame) : base(frame) { }
}
