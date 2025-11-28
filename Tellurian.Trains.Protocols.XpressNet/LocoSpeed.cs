using System.Globalization;

namespace Tellurian.Trains.Protocols.XpressNet;

public struct LocoSpeed : IEquatable<LocoSpeed>
{
    public static LocoSpeed FromCode(byte code, byte currentSpeedStep = ZeroStep) =>
        code switch
        {
            0 => new LocoSpeed(Speed14, currentSpeedStep),
            1 => new LocoSpeed(Speed27, currentSpeedStep),
            2 => new LocoSpeed(Speed28, currentSpeedStep),
            4 => new LocoSpeed(Speed126, currentSpeedStep),
            7 => throw new ArgumentOutOfRangeException(nameof(code), Resources.Strings.IsRailComEnabled),
            _ => throw new ArgumentOutOfRangeException(nameof(code), string.Format(CultureInfo.CurrentCulture,Resources.Strings.CodeIsInvalidSpeedCode, code))
        };

    public static LocoSpeed FromNumberOfSteps(byte numberOfSteps, byte currentSpeedStep = ZeroStep) =>
        numberOfSteps switch
        {
            14 => FromCode(0, currentSpeedStep),
            27 => FromCode(1, currentSpeedStep),
            28 => FromCode(2, currentSpeedStep),
            126 => FromCode(4, currentSpeedStep),
            128 => FromCode(4, currentSpeedStep),
            _ => throw new ArgumentOutOfRangeException(nameof(numberOfSteps), string.Format(CultureInfo.CurrentCulture, Resources.Strings.NumberIsInvalidNumberOfSpeedSteps, numberOfSteps))
        };

    private const byte ZeroStep = 0;
    public static byte EmergencyStopStep => 1;

    private readonly byte[] _stepsData;
    private byte _currentSpeedStep;

    private LocoSpeed((byte code, byte[] stepsData) config, byte currentSpeedStep =0)
    {
        Code = config.code;
        _stepsData = config.stepsData;
        _currentSpeedStep = currentSpeedStep < ZeroStep ? ZeroStep : currentSpeedStep > GetMaxSteps(_stepsData) ? GetMaxSteps(_stepsData) : currentSpeedStep;
    }

    public byte Code { get; }
    public byte Current { get { return _currentSpeedStep; } set { _currentSpeedStep = (byte)(value & 0x7F); } }
    public byte MaxSteps => GetMaxSteps(_stepsData);
    public byte Step(byte index) { return _stepsData[index]; }
    private void SetSpeed(float percentage) => Current = GetSpeed(percentage);
    private void SetSpeed(byte step) => Step(step > MaxSteps ? Step(MaxSteps) : step);
    private void SetMax() =>  Current = Step(MaxSteps);
    private void SetZero() => Current = ZeroStep;
    public byte GetSpeed(float percentage) => _stepsData[percentage < 0 ? 0 : percentage > 1 ? MaxSteps : (byte)(percentage * MaxSteps)];
    public bool Equals(LocoSpeed other) => other.Current == Current && other.MaxSteps == MaxSteps;
    public override bool Equals(object? obj) => obj is LocoSpeed other && Equals(other);
    public override int GetHashCode() => (Current.GetHashCode() / 2) + (MaxSteps.GetHashCode() / 2);
    public static bool operator ==(LocoSpeed left, LocoSpeed right) => left.Equals(right);
    public static bool operator !=(LocoSpeed left, LocoSpeed right) => !(left == right);
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0} of {1}", Current, MaxSteps);

    private static (byte code, byte[] stepsData) Speed14 =>
        (0x10, new byte[] { 0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

    private static (byte code, byte[] stepsData) Speed27 =>
        (0x11, new byte[] { 0, 2, 18, 3, 19, 4, 20, 5, 21, 6, 22, 7, 23, 8, 24, 9, 25, 10, 26, 11, 27, 12, 28, 13, 29, 14, 30, 15 });

    private static (byte code, byte[] stepsData) Speed28 =>
        (0x12, new byte[] { 0, 2, 18, 3, 19, 4, 20, 5, 21, 6, 22, 7, 23, 8, 24, 9, 25, 10, 26, 11, 27, 12, 28, 13, 29, 14, 30, 15, 31 });

    private static (byte code, byte[] stepsData) Speed126 =>
        (0x13, CreateSpeedSteps());
    private static byte[] CreateSpeedSteps()
    {
        var result = new byte[127];
        result[0] = 0;
        for (var i = 1; i <= 126; i++) { result[i] = (byte)(i + 1); }
        return result;
    }
    private static byte GetMaxSteps(byte[] stepsData) => (byte)(stepsData.Length - 1);
}

public static class LocoSpeedExtensions
{
    public static Interfaces.Locos.LocoSpeed Map(this LocoSpeed me) => Interfaces.Locos.LocoSpeed.Set((Interfaces.Locos.LocoSpeedSteps)me.MaxSteps, me.Current);
    public static LocoSpeed Map(this Interfaces.Locos.LocoSpeed me) => LocoSpeed.FromNumberOfSteps((byte)me.MaxSteps, me.CurrentStep);
}
