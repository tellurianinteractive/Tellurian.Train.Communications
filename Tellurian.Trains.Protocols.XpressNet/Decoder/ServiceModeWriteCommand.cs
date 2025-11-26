namespace Tellurian.Trains.Protocols.XpressNet.Decoder;

/// <summary>
/// Base class for service mode (programming track) write commands.
/// Standard XpressNet service mode supports CV 1-256.
/// </summary>
public abstract class ServiceModeWriteCommand : Commands.Command
{
    protected ServiceModeWriteCommand(byte identification, byte cvOrRegister, byte value)
        : base(0x23, [identification, cvOrRegister, value]) { }
}

/// <summary>
/// Register Mode write request (spec section 4.11).
/// Writes a value to a decoder register (1-8) using Register Mode on the programming track.
/// </summary>
/// <remarks>
/// Format: Header=0x23, Data=[0x12, REG, DATA]
/// Register range: 1-8
/// </remarks>
public sealed class ServiceModeWriteRegisterCommand : ServiceModeWriteCommand
{
    /// <summary>
    /// Creates a Register Mode write command.
    /// </summary>
    /// <param name="register">Register number (1-8)</param>
    /// <param name="value">Value to write (0-255)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when register is not in range 1-8</exception>
    public ServiceModeWriteRegisterCommand(byte register, byte value)
        : base(0x12, ValidateRegister(register), value) { }

    private static byte ValidateRegister(byte register) =>
        register is >= 1 and <= 8
            ? register
            : throw new ArgumentOutOfRangeException(nameof(register), "Register must be between 1 and 8");
}

/// <summary>
/// Direct Mode CV write request (spec section 4.12).
/// Writes a value to a CV (1-256) using Direct CV Mode on the programming track.
/// </summary>
/// <remarks>
/// Format: Header=0x23, Data=[0x16, CV, DATA]
/// CV range: 1-256 (CV 256 is sent as 0x00)
/// </remarks>
public sealed class ServiceModeWriteDirectCommand : ServiceModeWriteCommand
{
    /// <summary>
    /// Creates a Direct Mode CV write command.
    /// </summary>
    /// <param name="cv">CV number (1-256)</param>
    /// <param name="value">Value to write (0-255)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when CV is not in range 1-256</exception>
    public ServiceModeWriteDirectCommand(ushort cv, byte value)
        : base(0x16, ValidateAndEncodeCv(cv), value) { }

    private static byte ValidateAndEncodeCv(ushort cv) =>
        cv is >= 1 and <= 256
            ? (byte)(cv == 256 ? 0 : cv)
            : throw new ArgumentOutOfRangeException(nameof(cv), "CV must be between 1 and 256");
}

/// <summary>
/// Paged Mode write request (spec section 4.13).
/// Writes a value to a CV (1-256) using Paged Mode on the programming track.
/// The command station handles page register setup automatically.
/// </summary>
/// <remarks>
/// Format: Header=0x23, Data=[0x17, CV, DATA]
/// CV range: 1-256 (CV 256 is sent as 0x00)
/// </remarks>
public sealed class ServiceModeWritePagedCommand : ServiceModeWriteCommand
{
    /// <summary>
    /// Creates a Paged Mode write command.
    /// </summary>
    /// <param name="cv">CV number (1-256)</param>
    /// <param name="value">Value to write (0-255)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when CV is not in range 1-256</exception>
    public ServiceModeWritePagedCommand(ushort cv, byte value)
        : base(0x17, ValidateAndEncodeCv(cv), value) { }

    private static byte ValidateAndEncodeCv(ushort cv) =>
        cv is >= 1 and <= 256
            ? (byte)(cv == 256 ? 0 : cv)
            : throw new ArgumentOutOfRangeException(nameof(cv), "CV must be between 1 and 256");
}
