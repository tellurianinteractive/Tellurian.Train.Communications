using System.Diagnostics.CodeAnalysis;

namespace Tellurian.Trains.Communications.Interfaces.Decoder;

/// <summary>
/// Represents a Configuration Variable (CV) with its number and value.
/// CVs are used to configure DCC decoders.
/// </summary>
public readonly struct CV : IEquatable<CV>
{
    /// <summary>
    /// The CV number (1-1024).
    /// </summary>
    public int Number
    {
        get;
        init => field = value is >= 1 and <= 1024
            ? value
            : throw new ArgumentOutOfRangeException(nameof(Number), "CV number must be 1-1024");
    }

    /// <summary>
    /// The CV value (0-255).
    /// </summary>
    public byte Value { get; init; }

    /// <summary>
    /// Creates a CV with the specified number and value.
    /// </summary>
    /// <param name="number">CV number (1-1024)</param>
    /// <param name="value">CV value (0-255)</param>
    public CV(int number, byte value = 0)
    {
        Number = number;
        Value = value;
    }

    public bool Equals(CV other) => Number == other.Number && Value == other.Value;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is CV other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Number, Value);
    public override string ToString() => $"CV{Number}={Value}";

    public static bool operator ==(CV left, CV right) => left.Equals(right);
    public static bool operator !=(CV left, CV right) => !(left == right);
}

/// <summary>
/// Extension methods for creating CV instances.
/// </summary>
public static class CvExtensions
{
    /// <summary>
    /// Creates a CV from an integer number with optional value.
    /// </summary>
    /// <param name="number">CV number (1-1024)</param>
    /// <param name="value">CV value (0-255), defaults to 0</param>
    /// <returns>A new CV instance</returns>
    public static CV CV(this int number, byte value = 0) => new(number, value);
}
