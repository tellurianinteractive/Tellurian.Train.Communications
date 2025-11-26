# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET library for controlling model trains using Z21, LocoNet, and XpressNet protocols. The library provides protocol-agnostic interfaces that allow applications to control train operations, accessories, programming, and system monitoring.

## Querying Microsoft Documentation

You have access to MCP tools called `microsoft_docs_search`, `microsoft_docs_fetch`, and `microsoft_code_sample_search` - these 
tools allow you to search through and fetch Microsoft's latest official documentation and code samples, 
and that information might be more detailed or newer than what's in your training data set.

When handling questions around how to work with native Microsoft technologies, 
such as C#, F#, ASP.NET Core, Microsoft.Extensions, NuGet, Entity Framework, the `dotnet` runtime - please use 
these tools for research purposes when dealing with specific / narrowly defined questions that may occur.

## Documentation Guidelines

**IMPORTANT**: Exclude documents in folders named "Non-essential specifications", ""Processed documents" or similar. 
These contain supplementary information that is not required for development work. 
Focus on the essential specifications and code documentation instead.

## Build and Test Commands

### Building
```bash
dotnet build
```

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test Tellurian.Trains.Protocols.XpressNet.Tests
dotnet test Tellurian.Trains.Adapters.Z21.Tests
dotnet test Tellurian.Communications.Channels.Tests
dotnet test Tellurian.Protocols.LocoNet.Tests
dotnet test Tellurian.Trains.Interfaces.Tests

# List all tests
dotnet test --list-tests
```

### Package Creation
The build automatically creates NuGet packages for distributable projects.

## Architecture

The library follows a **layered architecture** with clear separation of concerns:

### 1. Transport Layer (`Tellurian.Communications.Channels`)
- **`ICommunicationsChannel`**: Protocol-agnostic communication interface using observer pattern
- **`UdpDataChannel`**: Concrete UDP implementation with asynchronous send/receive
- **`CommunicationResult`**: Result type representing success, failure, or no-operation outcomes
- **Observer Pattern**: Multiple observers can subscribe to receive communication notifications

### 2. Interface Layer (`Tellurian.Trains.Interfaces`)
- **Protocol-agnostic abstractions** that define the public API for clients
- **`ILocoControl`**: Locomotive control interface (Drive, EmergencyStop, SetFunction)
- **`ILocoDecoder`**: Decoder programming interface (ReadCV, WriteCV)
- **Data Types**: `LocoAddress`, `LocoDrive`, `LocoFunction`, `LocoSpeed`, `LocoDirection`
- **Notifications**: Abstract notification types for broadcasting state changes
- **Extensions**: Helper methods for data conversions (byte manipulation, CV addresses)

### 3. Protocol Layer
Each protocol implementation translates high-level commands to protocol-specific byte sequences:

#### XpressNet (`Tellurian.Trains.Protocols.XpressNet`)
- **Complete implementation** of the XpressNet protocol (Lenz, Z21)
- **Commands**: Locomotive control, track power, accessory control, CV programming
- **Notifications**: System state, locomotive info, programming responses, broadcasts
- **Message/Packet Structure**: Commands inherit from `Message`, packets handle XOR checksums
- **Key Types**: `Command`, `Notification`, `Packet` (with checksum handling)

#### LocoNet (`Tellurian.Protocols.LocoNet`)
- **Comprehensive implementation** (~75%) of Digitrax LocoNet Personal Use Edition 1.0
- **Complete Features**:
  - Power control (ON/OFF, emergency stop)
  - Full locomotive control (speed, direction, functions F0-F8)
  - Slot management (request, activate, dispatch, keep-alive)
  - Switch/turnout control with feedback
  - Sensor and occupancy detection
  - Programming track operations (CV read/write, all modes)
  - Operations mode programming (POM)
  - Multi-locomotive consisting (link/unlink, member control)
- **Commands**: All essential opcodes (0xA0-0xBF range, slot operations 0xE7/0xEF)
- **Notifications**: Slot data (14-byte), sensors, switches, acknowledgments, programming results
- **Data Types**: `SlotData`, `LocoAddress`, `AccessoryAddress`, status enums, programming types
- **Documentation**: See `README.md` and `API-REFERENCE.md` in LocoNet project folder
- **Not Implemented**: Fast clock (slot 123), raw DCC packets (0xED), rarely used

### 4. Adapter Layer (`Tellurian.Trains.Adapters.Z21`)
- **Z21 Adapter**: Wraps XpressNet and LocoNet protocols for Z21 command stations
- **Frame Handling**: Z21-specific frame structure with headers and protocol encapsulation
- **Notification Mapping**: Maps Z21 frames to protocol-agnostic interface notifications
- **Commands**: System queries (serial number, hardware info, system state)
- **Broadcast Subscriptions**: Configurable event filtering via `BroadcastSubjects`
- **Observer Pattern**: Distributes notifications to multiple observers

### Data Flow

```
Application
    � (uses ILocoControl, ILocoDecoder)
Adapter (Z21/LocoNet)
    � (translates to)
Protocol Layer (XpressNet/LocoNet Commands)
    � (sends via)
Transport Layer (UdpDataChannel)
    �
Physical Hardware (Z21, Digitrax, etc.)

(Notifications flow in reverse direction)
```

### Key Design Patterns

1. **Observer Pattern**: Used throughout for event notifications (communication results, state changes, notifications)
2. **Strategy Pattern**: Multiple protocol implementations behind common interfaces
3. **Command Pattern**: Protocol commands encapsulate operations as objects
4. **Factory Pattern**: `NotificationFactory` creates appropriate notification types from byte data
5. **Result Type**: `CommunicationResult` represents operation outcomes without exceptions

## Coding Conventions

This project follows strict C# coding standards documented in `Specifications/coding-conventions.md` and `Specifications/identifier-names.md`. Key requirements:

### Naming Conventions
- **PascalCase**: Classes, interfaces, public members, methods, properties, constants
- **camelCase**: Private/internal fields (prefixed with `_`), method parameters, local variables
- **Interface prefix**: All interfaces start with `I` (e.g., `ILocoControl`)
- **Static fields**: Private/internal static fields use `s_` prefix
- **Primary constructors**: PascalCase for record types, camelCase for class/struct types

### Language Features
- Target framework: **net10.0**
- **Modern C# features**: Use latest language version, collection expressions, raw string literals
- **`var`**: Use for temporary variables and LINQ results; avoid when type is not obvious
- **`required` properties**: Prefer over constructor parameters for mandatory initialization
- **File-scoped namespaces**: Use `namespace Foo;` instead of block syntax
- **Using directives**: Place outside namespace declarations

### Async and Error Handling
- Use `async`/`await` for I/O-bound operations (UDP communication)
- Use `ConfigureAwait(false)` to avoid deadlocks
- Only catch exceptions that can be properly handled
- Use specific exception types with meaningful messages

### Collections and Delegates
- Use **collection expressions** for initialization: `string[] items = ["a", "b"];`
- Prefer **`Func<>`** and **`Action<>`** over custom delegate types
- Use LINQ for collection manipulation

## Protocol-Specific Implementation Notes

### XpressNet Protocol
- **Checksum**: All packets require XOR checksum (handled by `Packet` class)
- **Message Structure**: Header byte + data bytes + checksum
- **Speed Steps**: Support for 14, 27, 28, or 126 speed steps
- **Address Ranges**: Short (1-127) and long (128-9999) addresses
- **Functions**: F0-F28 supported
- **CV Range**: 1-1024

### Z21 Frame Structure
- **Frame**: `[Length (2 bytes, little-endian)] [Header (2 bytes)] [Data (n bytes)]`
- **Encapsulation**: XpressNet and LocoNet commands wrapped in Z21 frames
- **NotificationFactory**: Parses incoming frames and creates appropriate notification objects

### LocoNet Protocol
- **Slot-based**: Locomotives managed via numbered slots
- **Checksum**: Different checksum algorithm than XpressNet
- **Operation Codes**: Defined in `OperationCodes` class
- **Incomplete**: Notification mapping to interface types needs completion

## Testing Strategy

Tests are organized by layer:
- `*.Tests` projects mirror the structure of their corresponding implementation projects
- Tests validate command byte generation, notification parsing, and protocol correctness
- Use concrete `UdpDataChannel` in integration tests; mock `ICommunicationsChannel` in unit tests

## Project Structure Summary

- **Tellurian.Communications.Channels**: UDP transport layer
- **Tellurian.Trains.Interfaces**: Protocol-agnostic interfaces and types
- **Tellurian.Trains.Protocols.XpressNet**: XpressNet protocol implementation (complete)
- **Tellurian.Protocols.LocoNet**: LocoNet protocol implementation (partial)
- **Tellurian.Trains.Adapters.Z21**: Z21 command station adapter
- **Specifications/**: Detailed coding conventions and naming standards
