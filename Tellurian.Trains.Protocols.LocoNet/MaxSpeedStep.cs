namespace Tellurian.Trains.Protocols.LocoNet;

public enum MaxSpeedStep
{
    Steps28 = 0b000,
    TrinarySteps28 = 0b001,
    Steps14 = 0b010,
    Steps128 = 0b011,
    Steps28Consisting = 0b100,
    Steps128Consisting = 0b111
}
