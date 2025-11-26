namespace Tellurian.Trains.Protocols.XpressNet.Decoder;

/// <summary>
/// Base class for service mode (programming track) read commands.
/// Standard XpressNet service mode supports CV 1-256.
/// </summary>
public abstract class ServiceModeReadCommand : Commands.Command
{
    protected ServiceModeReadCommand(byte identification, byte cvOrRegister)
        : base(0x22, [identification, cvOrRegister]) { }
}

/// <summary>
/// Register Mode read request (spec section 4.7).
/// Reads a decoder register (1-8) using Register Mode on the programming track.
/// </summary>
/// <remarks>
/// Format: Header=0x22, Data=[0x11, REG]
/// Register range: 1-8
/// </remarks>
public sealed class ServiceModeReadRegisterCommand : ServiceModeReadCommand
{
    /// <summary>
    /// Creates a Register Mode read command.
    /// </summary>
    /// <param name="register">Register number (1-8)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when register is not in range 1-8</exception>
    public ServiceModeReadRegisterCommand(byte register)
        : base(0x11, ValidateRegister(register)) { }

    private static byte ValidateRegister(byte register) =>
        register is >= 1 and <= 8
            ? register
            : throw new ArgumentOutOfRangeException(nameof(register), "Register must be between 1 and 8");
}

/// <summary>
/// Direct Mode CV read request (spec section 4.8).
/// Reads a CV (1-256) using Direct CV Mode on the programming track.
/// </summary>
/// <remarks>
/// Format: Header=0x22, Data=[0x15, CV]
/// CV range: 1-256 (CV 256 is sent as 0x00)
/// </remarks>
public sealed class ServiceModeReadDirectCommand : ServiceModeReadCommand
{
    /// <summary>
    /// Creates a Direct Mode CV read command.
    /// </summary>
    /// <param name="cv">CV number (1-256)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when CV is not in range 1-256</exception>
    public ServiceModeReadDirectCommand(ushort cv)
        : base(0x15, ValidateAndEncodeCv(cv)) { }

    private static byte ValidateAndEncodeCv(ushort cv) =>
        cv is >= 1 and <= 256
            ? (byte)(cv == 256 ? 0 : cv)
            : throw new ArgumentOutOfRangeException(nameof(cv), "CV must be between 1 and 256");
}

/// <summary>
/// Paged Mode read request (spec section 4.9).
/// Reads a CV (1-256) using Paged Mode on the programming track.
/// The command station handles page register setup automatically.
/// </summary>
/// <remarks>
/// Format: Header=0x22, Data=[0x14, CV]
/// CV range: 1-256 (CV 256 is sent as 0x00)
/// </remarks>
public sealed class ServiceModeReadPagedCommand : ServiceModeReadCommand
{
    /// <summary>
    /// Creates a Paged Mode read command.
    /// </summary>
    /// <param name="cv">CV number (1-256)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when CV is not in range 1-256</exception>
    public ServiceModeReadPagedCommand(ushort cv)
        : base(0x14, ValidateAndEncodeCv(cv)) { }

    private static byte ValidateAndEncodeCv(ushort cv) =>
        cv is >= 1 and <= 256
            ? (byte)(cv == 256 ? 0 : cv)
            : throw new ArgumentOutOfRangeException(nameof(cv), "CV must be between 1 and 256");
}
