namespace Tellurian.Trains.Protocols.XpressNet;

[Flags]
public enum LocoFunctionStates
{
    Off = 0x00,
    On = 0x40,
    Flip = 0x80
}
