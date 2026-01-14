# Z21 Adapter Documentation

This document provides detailed documentation for the `Tellurian.Trains.Adapters.Z21` library.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Dependency Injection Setup](#dependency-injection-setup)
- [How the Z21 Adapter Works](#how-the-z21-adapter-works)
- [Frame Structure](#frame-structure)
- [Commands](#commands)
- [Notifications](#notifications)
- [Broadcast Subscriptions](#broadcast-subscriptions)
- [Observer Pattern](#observer-pattern)
- [Error Handling](#error-handling)
- [Thread Safety](#thread-safety)
- [Accessory Addressing](#accessory-addressing)

## Architecture Overview

The Z21 adapter serves as a bridge between the protocol-agnostic interfaces defined in `Tellurian.Trains.Communications.Interfaces` and the Z21 command station hardware.

```
┌─────────────────────────────────────────────────────────────────┐
│                     Application Layer                           │
│                 (Your application code)                         │
└──────────────────────────┬──────────────────────────────────────┘
                           │ ILocoControl, IObservable<Notification>
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Adapter                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  • Implements ILocoControl                              │   │
│  │  • Implements IObservable<Notification>                 │   │
│  │  • Manages notification distribution                     │   │
│  │  • Coordinates XpressNet and LocoNet protocols          │   │
│  └─────────────────────────────────────────────────────────┘   │
└──────────────────────────┬──────────────────────────────────────┘
                           │ Frame encoding/decoding
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                   Protocol Layer                                │
│  ┌────────────────────┐    ┌────────────────────┐              │
│  │    XpressNet       │    │     LocoNet        │              │
│  │ (Loco control,     │    │ (Feedback,         │              │
│  │  Accessories)      │    │  Occupancy)        │              │
│  └────────────────────┘    └────────────────────┘              │
└──────────────────────────┬──────────────────────────────────────┘
                           │ Raw bytes
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                   Transport Layer                               │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              UdpDataChannel                              │   │
│  │  • Async send/receive                                    │   │
│  │  • Observer-based notifications                          │   │
│  │  • ICommunicationsChannel implementation                 │   │
│  └─────────────────────────────────────────────────────────┘   │
└──────────────────────────┬──────────────────────────────────────┘
                           │ UDP packets
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                   Z21 Hardware                                  │
│              (192.168.0.111:21105)                              │
└─────────────────────────────────────────────────────────────────┘
```

## Dependency Injection Setup

### Minimal Setup

The simplest configuration for a console application:

```csharp
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tellurian.Trains.Adapters.Z21;
using Tellurian.Trains.Communications.Channels;
using Tellurian.Trains.Communications.Interfaces.Locos;

var services = new ServiceCollection();

// Add logging (required dependency)
services.AddLogging(builder => builder.AddConsole());

// Configure the Z21 endpoint
var z21Endpoint = new IPEndPoint(IPAddress.Parse("192.168.0.111"), 21105);

// Register UDP channel
services.AddSingleton<ICommunicationsChannel>(sp =>
    new UdpDataChannel(
        localPort: 21105,
        remoteEndPoint: z21Endpoint,
        logger: sp.GetRequiredService<ILogger<UdpDataChannel>>()));

// Register adapter
services.AddSingleton<Adapter>();

var provider = services.BuildServiceProvider();
```

### ASP.NET Core / Generic Host Setup

For ASP.NET Core or hosted service applications:

```csharp
// In Program.cs or Startup.cs
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            // Read Z21 settings from configuration
            var z21Settings = context.Configuration
                .GetSection("Z21")
                .Get<Z21Settings>();

            var endpoint = new IPEndPoint(
                IPAddress.Parse(z21Settings.IpAddress),
                z21Settings.Port);

            // Register channel
            services.AddSingleton<ICommunicationsChannel>(sp =>
                new UdpDataChannel(
                    z21Settings.LocalPort,
                    endpoint,
                    sp.GetRequiredService<ILogger<UdpDataChannel>>()));

            // Register adapter
            services.AddSingleton<Adapter>();

            // Expose ILocoControl interface
            services.AddSingleton<ILocoControl>(sp =>
                sp.GetRequiredService<Adapter>());

            // Register background service for receiving notifications
            services.AddHostedService<Z21BackgroundService>();
        });
```

With `appsettings.json`:

```json
{
  "Z21": {
    "IpAddress": "192.168.0.111",
    "Port": 21105,
    "LocalPort": 21105
  }
}
```

### Custom IP Address Configuration

If your Z21 has a different IP address:

```csharp
// Using environment variable
var z21Ip = Environment.GetEnvironmentVariable("Z21_IP") ?? "192.168.0.111";
var z21Endpoint = new IPEndPoint(IPAddress.Parse(z21Ip), 21105);
```

### Factory Pattern for Multiple Z21 Units

```csharp
services.AddTransient<Func<string, Adapter>>(sp => ipAddress =>
{
    var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), 21105);
    var channel = new UdpDataChannel(
        localPort: 0, // OS assigns port
        remoteEndPoint: endpoint,
        logger: sp.GetRequiredService<ILogger<UdpDataChannel>>());

    return new Adapter(channel, sp.GetRequiredService<ILogger<Adapter>>());
});
```

## How the Z21 Adapter Works

### Message Flow

#### Sending Commands

1. **Application calls interface method** (e.g., `DriveAsync`)
2. **Adapter creates protocol command** (XpressNet `LocoDriveCommand`)
3. **Command wrapped in Z21 frame** with appropriate header
4. **Frame serialized to bytes** using little-endian encoding
5. **Bytes sent via UDP channel** to Z21

```csharp
// Example: DriveAsync flow
public Task<bool> DriveAsync(LocoAddress address, LocoDrive drive, CancellationToken cancellationToken)
{
    // 1. Create XpressNet command
    var xpressNetCommand = new LocoDriveCommand(address, drive.Speed.Map(), drive.Direction.Map());

    // 2. Wrap in Z21 frame and send
    return SendAsync(xpressNetCommand, cancellationToken);
}

public Task<bool> SendAsync(XpressNet.Command command, CancellationToken cancellationToken)
{
    // 3. Wrap XpressNet command in Z21 XpressNetCommand wrapper
    return SendAsync(new XpressNetCommand(command), cancellationToken);
}

public async Task<bool> SendAsync(Command command, CancellationToken cancellationToken)
{
    // 4. Convert to frame and get bytes
    var data = command.ToFrame().GetBytes();

    // 5. Send via UDP channel
    var result = await Channel.SendAsync(data, cancellationToken);
    return result.IsSuccess;
}
```

#### Receiving Notifications

1. **UDP packet received** by `UdpDataChannel`
2. **Channel notifies observers** with `CommunicationResult`
3. **Adapter parses Z21 frames** from received data
4. **NotificationFactory creates typed notification** based on header
5. **NotificationMapper converts to interface types**
6. **Application observers notified** with interface notifications

```csharp
private void ReceiveData(CommunicationResult result)
{
    // 1. Parse multiple frames from single UDP packet
    foreach (var frame in Frame.CreateMany(result))
    {
        // 2. Create Z21-specific notification
        var z21Notification = frame.Notification();

        // 3. Map to interface notification types
        var interfaceNotifications = z21Notification.Map();

        // 4. Notify all observers
        Observers.Notify(interfaceNotifications);
    }
}
```

### Class Responsibilities

| Class | Responsibility |
|-------|----------------|
| `Adapter` | Main entry point; implements `ILocoControl`, coordinates sending/receiving |
| `Frame` | Z21 frame structure; handles serialization/deserialization |
| `Command` | Base class for all commands; provides frame conversion |
| `Notification` | Base class for all notifications; wraps frame data |
| `NotificationFactory` | Creates specific notification types from frames |
| `NotificationMapper` | Converts Z21 notifications to interface notifications |
| `Observers<T>` | Thread-safe observer management and notification distribution |

## Frame Structure

The Z21 protocol uses a simple frame structure for all messages:

```
┌─────────────┬─────────────┬─────────────────────────┐
│  Length     │   Header    │        Data             │
│  (2 bytes)  │  (2 bytes)  │     (n bytes)           │
│ little-end  │ little-end  │   protocol payload      │
└─────────────┴─────────────┴─────────────────────────┘
     0-1          2-3             4 to (Length-1)
```

### Field Details

| Field | Size | Description |
|-------|------|-------------|
| Length | 2 bytes | Total frame length including all fields (little-endian) |
| Header | 2 bytes | Identifies command/notification type (little-endian) |
| Data | Variable | Protocol-specific payload |

### Header Values (FrameHeader enum)

| Header | Value | Description |
|--------|-------|-------------|
| `SerialNumber` | 0x10 | Request/response for serial number |
| `HardwareInfo` | 0x1A | Request/response for hardware information |
| `Logoff` | 0x30 | Disconnect from Z21 |
| `Xbus` | 0x40 | XpressNet protocol encapsulation |
| `SubscribeNotifications` | 0x50 | Subscribe to broadcast notifications |
| `BroadcastSubjects` | 0x51 | Current subscription confirmation |
| `LocoAddressMode` | 0x60 | Get locomotive address mode |
| `SetLocomotiveAddressMode` | 0x61 | Set locomotive address mode |
| `TurnoutAddressMode` | 0x70 | Get turnout address mode |
| `SetTurnoutAddressMode` | 0x71 | Set turnout address mode (see [Accessory Addressing](#accessory-addressing)) |
| `SystemStateChanged` | 0x84 | System state change notification |
| `SystemState` | 0x85 | Request system state |
| `LocoNetReceive` | 0xA0 | LocoNet message received |
| `LocoNetTransmit` | 0xA1 | LocoNet message transmitted |
| `LocoNetCommand` | 0xA2 | LocoNet command |
| `LocoNetDispatch` | 0xA3 | LocoNet dispatch |

### Frame Examples

**Get Serial Number Command:**
```
04 00 10 00
│  │  └──┴── Header: 0x0010 (SerialNumber)
└──┴─────── Length: 4 bytes
```

**XpressNet Command (Track Power On):**
```
07 00 40 00 21 81 A0
│  │  │  │  └──┴──┴── XpressNet payload
│  │  └──┴────────── Header: 0x0040 (Xbus)
└──┴──────────────── Length: 7 bytes
```

## Commands

### Z21-Specific Commands

```csharp
// Get hardware serial number
await adapter.SendAsync(new GetSerialNumberCommand());

// Get hardware type and firmware version
await adapter.SendAsync(new GetHardwareInfoCommand());

// Get current system state (voltage, current, temperature)
await adapter.SendAsync(new GetSystemStateCommand());

// Subscribe to notifications
await adapter.SendAsync(new SubscribeNotificationsCommand(BroadcastSubjects.All));

// Get current broadcast subscriptions
await adapter.SendAsync(new GetSubscribedNotificationsCommand());

// Disconnect from Z21
await adapter.SendAsync(new LogOffCommand());
```

### Locomotive Control Commands

These are exposed via the `ILocoControl` interface:

```csharp
// Drive a locomotive
var address = new LocoAddress(3);
var drive = new LocoDrive(new LocoSpeed(64, SpeedSteps.S128), LocoDirection.Forward);
await adapter.DriveAsync(address, drive);

// Emergency stop a specific locomotive
await adapter.EmergencyStopAsync(address);

// Control functions (F0-F28)
await adapter.SetFunctionAsync(address, new LocoFunction(0, true));  // Lights on
await adapter.SetFunctionAsync(address, new LocoFunction(1, true));  // Sound on
await adapter.SetFunctionAsync(address, new LocoFunction(1, false)); // Sound off
```

### XpressNet Commands via Adapter

The adapter can send raw XpressNet commands:

```csharp
using Tellurian.Trains.Protocols.XpressNet.Commands;

// Track power commands
await adapter.SendAsync(new TrackPowerOnCommand());
await adapter.SendAsync(new TrackPowerOffCommand());
await adapter.SendAsync(new EmergencyStopCommand());

// Get locomotive information
await adapter.GetLocoInfoAsync(new LocoAddress(3));
```

## Notifications

### System State Notification

Contains Z21 hardware status:

```csharp
public class MyObserver : IObserver<Notification>
{
    public void OnNext(Notification notification)
    {
        if (notification is SystemStateNotification state)
        {
            Console.WriteLine($"Main Current: {state.MainCurrent}A");
            Console.WriteLine($"Track Voltage: {state.TrackVoltage}V");
            Console.WriteLine($"Temperature: {state.InternalTemperature}°C");
            Console.WriteLine($"Status: {state.Status}");
        }
    }
}
```

### XpressNet Notifications

Locomotive and system events from XpressNet protocol:

| Notification Type | Description |
|-------------------|-------------|
| `LocoInfoNotification` | Locomotive speed, direction, functions |
| `TrackPowerOnBroadcast` | Track power enabled |
| `TrackPowerOffBroadcast` | Track power disabled |
| `EmergencyStopBroadcast` | Global emergency stop active |
| `TrackShortCircuitNotification` | Short circuit detected |
| `ProgrammingModeBroadcast` | Entered programming mode |

### LocoNet Notifications

Feedback and occupancy from LocoNet protocol:

| Notification Type | Description |
|-------------------|-------------|
| `SensorNotification` | Track sensor state change |
| `TurnoutNotification` | Turnout position feedback |
| `OccupancyNotification` | Block occupancy change |

## Broadcast Subscriptions

Configure which notifications you want to receive:

```csharp
// Subscribe to specific events
var subjects = BroadcastSubjects.SystemStateChanges
             | BroadcastSubjects.ChangedLocomotiveInfo;
await adapter.SendAsync(new SubscribeNotificationsCommand(subjects));

// Subscribe to all events
await adapter.SendAsync(new SubscribeNotificationsCommand(BroadcastSubjects.All));

// Unsubscribe from all events
await adapter.SendAsync(new SubscribeNotificationsCommand(BroadcastSubjects.None));
```

### Available Broadcast Subjects

| Subject | Description |
|---------|-------------|
| `None` | No subscriptions |
| `RunningAndSwitching` | LocoNet turnout notifications |
| `RbusFeedback` | R-Bus detector changes |
| `SystemStateChanges` | Power, emergency stop, short circuit |
| `ChangedLocomotiveInfo` | All locomotive changes |
| `LocoNetExceptLocomotivesAndSwitches` | General LocoNet messages |
| `LocoNetLocomotiveSpecific` | LocoNet locomotive messages |
| `LocoNetTurnouts` | LocoNet turnout messages |
| `OccupiedStretch` | Block occupancy |
| `All` | All of the above |

## Observer Pattern

The adapter implements `IObservable<Notification>` for the standard .NET observer pattern:

### Basic Observer Implementation

```csharp
public class LocoObserver : IObserver<Notification>
{
    public void OnNext(Notification notification)
    {
        // Handle notification
        switch (notification)
        {
            case LocoInfo info:
                Console.WriteLine($"Loco {info.Address}: Speed={info.Speed}");
                break;
            case TrackPowerOn:
                Console.WriteLine("Track power ON");
                break;
            case TrackPowerOff:
                Console.WriteLine("Track power OFF");
                break;
        }
    }

    public void OnError(Exception error)
    {
        Console.WriteLine($"Error: {error.Message}");
    }

    public void OnCompleted()
    {
        Console.WriteLine("Connection closed");
    }
}
```

### Using the Observer

```csharp
var adapter = provider.GetRequiredService<Adapter>();
var observer = new LocoObserver();

// Subscribe (returns IDisposable for unsubscription)
var subscription = adapter.Subscribe(observer);

// Start receiving
await adapter.StartReceiveAsync();

// Later: unsubscribe
subscription.Dispose();
```

### Multiple Observers

```csharp
// Multiple observers can subscribe
var loggingObserver = new LoggingObserver();
var uiObserver = new UIUpdateObserver();
var persistenceObserver = new DatabaseObserver();

adapter.Subscribe(loggingObserver);
adapter.Subscribe(uiObserver);
adapter.Subscribe(persistenceObserver);

// All observers receive all notifications
await adapter.StartReceiveAsync();
```

## Error Handling

### Communication Errors

```csharp
public class ErrorHandlingObserver : IObserver<Notification>
{
    public void OnError(Exception error)
    {
        switch (error)
        {
            case SocketException socketEx:
                Console.WriteLine($"Network error: {socketEx.Message}");
                // Attempt reconnection
                break;

            case TimeoutException:
                Console.WriteLine("Communication timeout");
                break;

            default:
                Console.WriteLine($"Unexpected error: {error.Message}");
                throw error;
        }
    }
    // ... other methods
}
```

### Command Result Handling

```csharp
var success = await adapter.DriveAsync(address, drive);
if (!success)
{
    Console.WriteLine("Failed to send drive command");
    // Handle failure (retry, notify user, etc.)
}
```

## Thread Safety

The adapter is designed for thread-safe operation:

- **Observer management**: Adding/removing observers is thread-safe
- **Notification delivery**: Observers are notified on background threads
- **Command sending**: Multiple commands can be sent concurrently

### Observer Thread Safety Requirements

Your observers must handle thread safety:

```csharp
public class ThreadSafeObserver : IObserver<Notification>
{
    private readonly object _lock = new();
    private readonly List<Notification> _notifications = new();

    public void OnNext(Notification notification)
    {
        lock (_lock)
        {
            _notifications.Add(notification);
        }

        // Or use thread-safe collections
        // _concurrentQueue.Enqueue(notification);
    }
}
```

### UI Thread Marshaling

For UI applications, marshal to the UI thread:

```csharp
public class WpfObserver : IObserver<Notification>
{
    private readonly Dispatcher _dispatcher;

    public WpfObserver(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public void OnNext(Notification notification)
    {
        _dispatcher.Invoke(() =>
        {
            // Update UI here
        });
    }
}
```

## Accessory Addressing

### User vs Wire Addresses

This library uses **1-based user addresses** (1-2048) for accessories and turnouts. This matches how users typically configure their systems - turnout #1 is address 1, not address 0.

Internally, the protocol layers convert to **0-based wire addresses** (0-2047) when encoding commands. This conversion is automatic and transparent to your application code.

```csharp
// Your code uses user addresses (1-based)
var turnoutAddress = AccessoryAddress.From(100);  // Turnout #100
await adapter.SetThrownAsync(turnoutAddress, true);

// Internally converted to wire address 99 for the protocol
```

### Z21 Module Addressing (+4 Shift)

There is a historical ambiguity in the DCC specification about how accessory addresses map to decoder modules:

| System | Module 0 Contains | Effect |
|--------|-------------------|--------|
| **Roco/Z21** | Addresses 1-4 | Standard for Z21 users |
| **Other manufacturers** | Addresses 5-8 | Shifted by 4 |

This means a turnout programmed as address 1 on another system may respond to address 5 on a Z21, and vice versa.

### Fixing Address Mismatches

The Z21 has a configuration option **"DCC-Weichenadressverschiebung +4"** (DCC turnout address shift +4) that compensates for this:

- **Enable** if your accessories are off by 4 compared to another system
- **Configure via** Z21 Maintenance software (not through the protocol API)
- **Effect**: Z21 internally adds 4 to all accessory addresses before sending

**Note**: This library does not handle the +4 shift - it's a hardware setting. Your application should use the same address scheme as displayed on your Z21 or throttle.

## Resource Management

Always dispose the adapter when done:

```csharp
// Using statement
await using var adapter = provider.GetRequiredService<Adapter>();
// ... use adapter ...
// Automatically disposed

// Or manual disposal
var adapter = provider.GetRequiredService<Adapter>();
try
{
    // ... use adapter ...
}
finally
{
    adapter.Dispose();
}
```

### Hosted Service Example

```csharp
public class Z21BackgroundService : BackgroundService
{
    private readonly Adapter _adapter;

    public Z21BackgroundService(Adapter adapter)
    {
        _adapter = adapter;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Subscribe to notifications
        await _adapter.SendAsync(new SubscribeNotificationsCommand(BroadcastSubjects.All));

        // Start receiving
        await _adapter.StartReceiveAsync(stoppingToken);

        // Keep running until cancelled
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _adapter.Dispose();
        base.Dispose();
    }
}
```
