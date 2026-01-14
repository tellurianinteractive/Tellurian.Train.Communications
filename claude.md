# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET library for controlling model trains using Z21, LocoNet, and XpressNet protocols. The library provides protocol-agnostic interfaces that allow applications to control train operations, accessories, programming, and system monitoring.

## Build and Test Commands

```bash
# Build the solution
dotnet build

# Apply code analysis fixes after building
dotnet format analyzers --severity info

# Run all tests (uses MSTest.Sdk with Microsoft Testing Platform)
dotnet test

# Run tests for a specific project
dotnet test --project Tellurian.Trains.Protocols.XpressNet.Tests
dotnet test --project Tellurian.Trains.Adapters.Z21.Tests

# Run from within a test project directory
cd Tellurian.Trains.Communications.Interfaces.Tests
dotnet run

# List all tests in a project
dotnet test --project Tellurian.Trains.Communications.Interfaces.Tests --list-tests
```

## .NET Version

This project targets **net10.0** with the latest C# language version. This is configured globally in `Directory.Build.props`.

## Architecture

The library follows a **layered architecture**:

```
Application
    ↓ (uses ILoco, IAccessory, ISwitch, IDecoder)
Adapter Layer (Z21/LocoNet)
    ↓ (translates to)
Protocol Layer (XpressNet/LocoNet Commands)
    ↓ (sends via)
Transport Layer (UdpDataChannel or SerialDataChannel)
    ↓
Physical Hardware (Z21, Digitrax, etc.)

(Notifications flow in reverse direction)
```

## Project Structure

- **Tellurian.Trains.Communications.Channels**: UDP/serial transport layer - see `Tellurian.Trains.Communications.Channels/CLAUDE.md`
- **Tellurian.Trains.Communications.Interfaces**: Protocol-agnostic interfaces and types - see `Tellurian.Trains.Communications.Interfaces/CLAUDE.md`
- **Tellurian.Trains.Protocols.XpressNet**: XpressNet protocol implementation - see `Tellurian.Trains.Protocols.XpressNet/CLAUDE.md`
- **Tellurian.Trains.Protocols.LocoNet**: LocoNet protocol implementation - see `Tellurian.Trains.Protocols.LocoNet/CLAUDE.md`
- **Tellurian.Trains.Adapters.Z21**: Z21 command station adapter - see `Tellurian.Trains.Adapters.Z21/CLAUDE.md`
- **Tellurian.Trains.Adapters.LocoNet**: LocoNet command station adapter - see `Tellurian.Trains.Adapters.LocoNet/CLAUDE.md`
- **Specifications/**: Protocol specification PDFs (reference only, not for development)

## Key Design Patterns

1. **Observer Pattern**: Event notifications (communication results, state changes, notifications)
2. **Strategy Pattern**: Multiple protocol implementations behind common interfaces
3. **Command Pattern**: Protocol commands encapsulate operations as objects
4. **Factory Pattern**: `NotificationFactory` creates notification types from byte data
5. **Result Type**: `CommunicationResult` represents operation outcomes without exceptions

## Important Conventions

### Accessory Addressing
User-facing accessory addresses are **1-based** (1-2048). Protocol implementations convert to 0-based wire addresses internally.

### Logging
Use `Microsoft.Extensions.Logging`. Precede logging statements with checks like `if (logger.IsEnabled(LogLevel.Debug))`.

### Testing
- Tests use MSTest.Sdk with Microsoft Testing Platform (configured in `global.json`)
- Use `Assert.Throws` (not `[ExpectedException]` or `Assert.ThrowsException`)
- Use `InternalsVisibleTo` for test projects to access internal members

## Additional Context

- Each project has its own `CLAUDE.md` with project-specific implementation details
- Agent guidelines in `.agent_guidelines/` folder provide detailed conventions
- **Exclude** documents in `Specifications/Non-essential specifications/` and `Specifications/Processed documents/` - focus on code
