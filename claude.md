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

### 1. Transport Layer
**Project**: `Tellurian.Communications.Channels`

Protocol-agnostic UDP communication infrastructure.

See `Tellurian.Communications.Channels/CLAUDE.md` for details.

### 2. Interface Layer
**Project**: `Tellurian.Trains.Interfaces`

Protocol-agnostic abstractions for client applications.

See `Tellurian.Trains.Interfaces/CLAUDE.md` for details.

### 3. Protocol Layer
**Projects**: `Tellurian.Trains.Protocols.XpressNet`, `Tellurian.Protocols.LocoNet`

Protocol implementations that translate high-level commands to protocol-specific byte sequences.

- See `Tellurian.Trains.Protocols.XpressNet/CLAUDE.md` for XpressNet details.
- See `Tellurian.Protocols.LocoNet/CLAUDE.md` for LocoNet details.

### 4. Adapter Layer
**Project**: `Tellurian.Trains.Adapters.Z21`

Adapter for Z21 command stations.

See `Tellurian.Trains.Adapters.Z21/CLAUDE.md` for details.

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

This project follows strict C# coding standards documented by Microsoft.

### Naming Conventions
Ask Microsoft for details on C# naming conventions.
 
### Language Features
- Target framework: **net10.0**
- **Modern C# features**: Use C# 10 and later features as documented by Microsoft.

### Async and Error Handling
- Use `async`/`await` for any I/O-bound operations
- Use `ConfigureAwait(false)` to avoid deadlocks
- Only catch exceptions that can be properly handled
- Use specific exception types with meaningful messages

### Collections and Delegates
- Use **collection expressions** for initialization: `string[] items = ["a", "b"];`
- Prefer **`Func<>`** and **`Action<>`** over custom delegate types
- Use LINQ for collection manipulation

## Testing Strategy

Tests are organized by layer:
- `*.Tests` projects mirror the structure of their corresponding implementation projects
- Tests validate command byte generation, notification parsing, and protocol correctness
- Use concrete `UdpDataChannel` in integration tests; mock `ICommunicationsChannel` in unit tests

## Project Structure Summary

- **Tellurian.Communications.Channels**: UDP transport layer - see `Tellurian.Communications.Channels/CLAUDE.md`
- **Tellurian.Trains.Interfaces**: Protocol-agnostic interfaces and types - see `Tellurian.Trains.Interfaces/CLAUDE.md`
- **Tellurian.Trains.Protocols.XpressNet**: XpressNet protocol implementation - see `Tellurian.Trains.Protocols.XpressNet/CLAUDE.md`
- **Tellurian.Protocols.LocoNet**: LocoNet protocol implementation - see `Tellurian.Protocols.LocoNet/CLAUDE.md`
- **Tellurian.Trains.Adapters.Z21**: Z21 command station adapter - see `Tellurian.Trains.Adapters.Z21/CLAUDE.md`
- **Specifications/**: Detailed coding conventions and naming standards

Each project has its own `CLAUDE.md` file with project-specific implementation details.
