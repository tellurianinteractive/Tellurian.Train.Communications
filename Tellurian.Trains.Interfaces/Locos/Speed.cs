using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
public readonly struct Speed : IEquatable<Speed>
{
    public static Speed Set126(byte step) => new(LocoSpeedSteps.Steps126, step);
    public static Speed Set(LocoSpeedSteps steps, byte step) => new(steps, step);

    private Speed(LocoSpeedSteps maxStep, byte currentStep)
    {
        _maxSteps = (byte)maxStep;
        _currentStep = currentStep;
    }

    [DataMember(Name = "CurrentStep")]
    private readonly byte _currentStep;

    [DataMember(Name = "MaxSteps")]
    private readonly byte _maxSteps;

    public LocoSpeedSteps MaxSteps => _maxSteps.AsLocoSpeedSteps();
    public byte CurrentStep => _currentStep;
    public bool Equals(Speed other) => other._currentStep == _currentStep && other._maxSteps == _maxSteps;
    public override bool Equals(object? obj) => obj is Speed other && Equals(other);
    public override int GetHashCode() => (_currentStep.GetHashCode() / 2) + (_maxSteps.GetHashCode() / 2);
    public static bool operator ==(Speed left, Speed right) => left.Equals(right);
    public static bool operator !=(Speed left, Speed right) => !(left == right);
}

public static class LocoSpeedExtensions
{
    public static Speed ChangeTo(this Speed me, float percent) =>
        Speed.Set(me.MaxSteps, (byte)Math.Round((byte)me.MaxSteps * percent.BetweenZeroAndOne()));

    private static float BetweenZeroAndOne(this float percent) => percent < 0 ? 0 : percent > 1 ? 1 : percent;
    internal static LocoSpeedSteps AsLocoSpeedSteps(this byte value) =>
        value switch
        {
            14 => LocoSpeedSteps.Steps14,
            27 => LocoSpeedSteps.Steps27,
            28 => LocoSpeedSteps.Steps28,
            _ => LocoSpeedSteps.Steps126
        };
}

#pragma warning disable CA1028, CA1717 
public enum LocoSpeedSteps : byte
{
    Steps14 = 14,
    Steps27 = 27,
    Steps28 = 28,
    Steps126 = 126
}