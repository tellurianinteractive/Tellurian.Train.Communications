# Tellurian.Communications.Channels

This project provides the **Transport Layer** for the Tellurian Trains Control library.

## Purpose

Provides protocol-agnostic communication infrastructure for UDP-based communication with model train control hardware.

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
- Observer notification delivery
- Error handling and edge cases
- Thread safety of observer management

## Usage Notes

- Use `ConfigureAwait(false)` when awaiting communication operations
- Observers are notified on background threads - ensure thread-safe handling
- The channel is reusable - no need to recreate for each operation
- Dispose the channel when done to release network resources
