namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Search direction for address inquiry commands.
/// </summary>
public enum SearchDirection
{
    /// <summary>
    /// Forward search - get next member/address.
    /// </summary>
    Forward,

    /// <summary>
    /// Backward search - get previous member/address.
    /// Note: Currently only supported in future command station versions.
    /// </summary>
    Backward
}

/// <summary>
/// Address inquiry member of a Multi-Unit request (spec section 4.25.1).
/// Requests the next or previous member in a Multi-Unit consist.
/// </summary>
/// <remarks>
/// Format: Header=0xE4, Data=[0x01+R, MTR, AH, AL]
/// - R=01: Forward search (next member)
/// - R=10: Backward search (previous member) - future feature
///
/// If AH/AL is 0x00, returns the first locomotive in the Multi-Unit.
///
/// Response: AddressRetrievalNotification (section 3.17)
/// </remarks>
public sealed class AddressInquiryMultiUnitMemberCommand : Command
{
    /// <summary>
    /// Creates a command to query Multi-Unit members.
    /// </summary>
    /// <param name="multiUnitAddress">Multi-Unit base address (1-99)</param>
    /// <param name="startAddress">Starting locomotive address, or null to get first member</param>
    /// <param name="direction">Search direction (forward or backward)</param>
    public AddressInquiryMultiUnitMemberCommand(byte multiUnitAddress, LocoAddress? startAddress = null,
        SearchDirection direction = SearchDirection.Forward)
        : base(0xE4, GetData(multiUnitAddress, startAddress, direction))
    {
        ValidateMultiUnitAddress(multiUnitAddress);
    }

    private static byte[] GetData(byte multiUnitAddress, LocoAddress? startAddress, SearchDirection direction)
    {
        var identification = direction == SearchDirection.Forward ? (byte)0x01 : (byte)0x02;
        byte ah = 0x00, al = 0x00;

        if (startAddress.HasValue)
        {
            var addrBytes = startAddress.Value.GetBytesAccordingToXpressNet();
            ah = addrBytes[0];
            al = addrBytes[1];
        }

        return [identification, multiUnitAddress, ah, al];
    }

    private static void ValidateMultiUnitAddress(byte address)
    {
        if (address < 1 || address > 99)
            throw new ArgumentOutOfRangeException(nameof(address), "Multi-Unit address must be between 1 and 99");
    }
}

/// <summary>
/// Address inquiry Multi-Unit request (spec section 4.25.2).
/// Requests the next or previous Multi-Unit base address.
/// </summary>
/// <remarks>
/// Format: Header=0xE2, Data=[0x03+R, MTR]
/// - R=011: Forward search (next MTR)
/// - R=100: Backward search (previous MTR) - future feature
///
/// If MTR is 0x00, returns the first Multi-Unit address.
///
/// Response: AddressRetrievalNotification (section 3.17)
/// </remarks>
public sealed class AddressInquiryMultiUnitCommand : Command
{
    /// <summary>
    /// Creates a command to query Multi-Unit base addresses.
    /// </summary>
    /// <param name="startMultiUnitAddress">Starting MTR address (0 to get first, 1-99 to get next)</param>
    /// <param name="direction">Search direction (forward or backward)</param>
    public AddressInquiryMultiUnitCommand(byte startMultiUnitAddress = 0,
        SearchDirection direction = SearchDirection.Forward)
        : base(0xE2, GetData(startMultiUnitAddress, direction))
    {
        if (startMultiUnitAddress > 99)
            throw new ArgumentOutOfRangeException(nameof(startMultiUnitAddress),
                "Multi-Unit address must be between 0 and 99");
    }

    private static byte[] GetData(byte startMultiUnitAddress, SearchDirection direction)
    {
        var identification = direction == SearchDirection.Forward ? (byte)0x03 : (byte)0x04;
        return [identification, startMultiUnitAddress];
    }
}

/// <summary>
/// Address inquiry locomotive at command station stack request (spec section 4.25.3).
/// Requests the next or previous locomotive address in the command station database.
/// </summary>
/// <remarks>
/// Format: Header=0xE3, Data=[0x05+R, AH, AL]
/// - R=01: Forward search (next address)
/// - R=10: Backward search (previous address) - future feature
///
/// If AH/AL is 0x00, returns the first locomotive in the database.
///
/// Note: Stack entries are sorted by time of first use, not by address number.
///
/// Response: AddressRetrievalNotification (section 3.17)
/// </remarks>
public sealed class AddressInquiryStackCommand : Command
{
    /// <summary>
    /// Creates a command to query the command station stack.
    /// </summary>
    /// <param name="startAddress">Starting locomotive address, or null to get first entry</param>
    /// <param name="direction">Search direction (forward or backward)</param>
    public AddressInquiryStackCommand(LocoAddress? startAddress = null,
        SearchDirection direction = SearchDirection.Forward)
        : base(0xE3, GetData(startAddress, direction)) { }

    private static byte[] GetData(LocoAddress? startAddress, SearchDirection direction)
    {
        var identification = direction == SearchDirection.Forward ? (byte)0x05 : (byte)0x06;
        byte ah = 0x00, al = 0x00;

        if (startAddress.HasValue)
        {
            var addrBytes = startAddress.Value.GetBytesAccordingToXpressNet();
            ah = addrBytes[0];
            al = addrBytes[1];
        }

        return [identification, ah, al];
    }
}

/// <summary>
/// Delete locomotive from command station stack request (spec section 4.26).
/// Removes a locomotive address from the command station database.
/// </summary>
/// <remarks>
/// Format: Header=0xE3, Data=[0x44, AH, AL]
///
/// Before deleting an entry, verify it's not being controlled by another device,
/// otherwise it would immediately be re-added.
///
/// Most command stations have limited space for locomotive entries. When full,
/// an entry must be removed before a new one can be added.
///
/// Response: None
/// </remarks>
public sealed class DeleteLocoFromStackCommand : Command
{
    /// <summary>
    /// Creates a command to delete a locomotive from the command station stack.
    /// </summary>
    /// <param name="address">Locomotive address to delete (1-9999)</param>
    public DeleteLocoFromStackCommand(LocoAddress address)
        : base(0xE3, GetData(address)) { }

    private static byte[] GetData(LocoAddress address)
    {
        var addrBytes = address.GetBytesAccordingToXpressNet();
        return [0x44, addrBytes[0], addrBytes[1]];
    }
}
