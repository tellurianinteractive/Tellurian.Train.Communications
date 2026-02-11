# Tellurian.Communications.Channels

This project provides the **Transport Layer** for the Tellurian Trains Control library.

## Purpose

Provides protocol-agnostic communication infrastructure for UDP, TCP, and serial communication with model train control hardware.

## Key Components

### ICommunicationsChannel
- **Protocol-agnostic communication interface** using observer pattern
- Supports multiple observers for communication events
- Thread-safe notification distribution

### UdpDataChannel
- **Concrete UDP implementation** with asynchronous send/receive
- Non-blocking operations using `async`/`await`
- Configurable endpoints (IP address and port)
- Automatic buffer management

### TcpLocoNetChannel
- **LoconetOverTcp implementation** (Stefan Bormann protocol) over TCP
- Exchanges ASCII-encoded hex lines: `RECEIVE`/`SEND` with hex-encoded LocoNet bytes
- Handles protocol tokens: `VERSION`, `SENT OK/ERROR`, `TIMESTAMP`, `BREAK`, `ERROR`
- Uses `ITcpStreamAdapter` abstraction for testability (with `MockTcpStreamAdapter` for tests)

### UdpLocoNetChannel
- **LocoNet over multicast UDP** — raw binary LocoNet messages as UDP datagrams (no text framing)
- Supports both **loconetd** (Glenn Butcher, multicast 225.0.0.2:4501) and **GCA101** (Rocrail/Peter Giling, multicast 224.0.0.1:1235)
- Optional LocoNet checksum validation (opcode MSB=1, XOR of all bytes == 0xFF)
- Uses `IUdpLocoNetAdapter` abstraction for testability (with `MockUdpLocoNetAdapter` for tests)
- Auto-joins multicast group on first send or receive

### SerialDataChannel
- **Serial port implementation** with protocol-specific message framing
- Uses `ISerialPortAdapter` and `IByteStreamFramer` abstractions
- Suitable for direct LocoBuffer-USB connections

### CommunicationResult
- **Result type** representing success, failure, or no-operation outcomes
- Avoids exception-based control flow for normal operation
- Provides detailed error information when needed

## Design Patterns

- **Observer Pattern**: Multiple observers can subscribe to receive communication notifications
- **Result Type**: Operation outcomes represented without exceptions for better control flow
- **Async/Await**: All I/O operations are asynchronous to prevent blocking

## Testing

The `Tellurian.Communications.Channels.Tests` project validates:
- UDP send/receive operations
- UDP LocoNet multicast channel with checksum validation
- TCP LoconetOverTcp protocol parsing and lifecycle
- Observer notification delivery
- Error handling and edge cases
- Thread safety of observer management

## Usage Notes

- Use `ConfigureAwait(false)` when awaiting communication operations
- Observers are notified on background threads - ensure thread-safe handling
- The channel is reusable - no need to recreate for each operation
- Dispose the channel when done to release network resources
