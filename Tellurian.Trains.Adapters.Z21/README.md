# Tellurian.Trains.Adapters.Z21

A .NET adapter for the **Roco/Fleischmann Z21** digital command station, providing a high-level API for locomotive control, accessory switching, and system monitoring over UDP.

## Features

- **Locomotive Control**: Drive, emergency stop, and function control via `ILocoControl`
- **Protocol Support**: Encapsulates both XpressNet and LocoNet protocols
- **Real-time Notifications**: Observer-based notification system for state changes
- **System Monitoring**: Track voltage, current, temperature, and status
- **Broadcast Filtering**: Configurable subscription to specific event types

## Quick Start

### Dependency Injection Setup

Add the Z21 adapter to your application in `Program.cs`:

```csharp
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tellurian.Trains.Adapters.Z21;
using Tellurian.Trains.Communications.Channels;
using Tellurian.Trains.Interfaces.Locos;

var services = new ServiceCollection();

// Add logging
services.AddLogging(builder => builder.AddConsole());

// Configure Z21 endpoint (default: 192.168.0.111:21105)
var z21Endpoint = new IPEndPoint(IPAddress.Parse("192.168.0.111"), 21105);

// Register the UDP communication channel
services.AddSingleton<ICommunicationsChannel>(sp =>
    new UdpDataChannel(
        localPort: 21105,
        remoteEndPoint: z21Endpoint,
        logger: sp.GetRequiredService<ILogger<UdpDataChannel>>()));

// Register the Z21 adapter
services.AddSingleton<Adapter>();

// Register ILocoControl interface (the Adapter implements it)
services.AddSingleton<ILocoControl>(sp => sp.GetRequiredService<Adapter>());

var provider = services.BuildServiceProvider();
```

### Basic Usage

```csharp
using Tellurian.Trains.Adapters.Z21;
using Tellurian.Trains.Interfaces.Locos;

// Get the adapter from DI
var adapter = provider.GetRequiredService<Adapter>();

// Subscribe to notifications
adapter.Subscribe(new MyNotificationObserver());

// Subscribe to Z21 broadcast subjects
await adapter.SendAsync(new SubscribeNotificationsCommand(BroadcastSubjects.All));

// Start receiving notifications
await adapter.StartReceiveAsync();

// Control a locomotive
var locoAddress = new LocoAddress(3); // DCC address 3
var drive = new LocoDrive(new LocoSpeed(50, SpeedSteps.S128), LocoDirection.Forward);
await adapter.DriveAsync(locoAddress, drive);

// Toggle function (e.g., lights)
await adapter.SetFunctionAsync(locoAddress, new LocoFunction(0, true));

// Emergency stop
await adapter.EmergencyStopAsync(locoAddress);
```

### Notification Observer

```csharp
using Tellurian.Trains.Interfaces;

public class MyNotificationObserver : IObserver<Notification>
{
    public void OnNext(Notification notification)
    {
        Console.WriteLine($"Received: {notification}");
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

## Z21 Hardware Connection

| Setting | Default Value |
|---------|---------------|
| IP Address | 192.168.0.111 |
| UDP Port | 21105 |
| Protocol | UDP (bidirectional) |

Ensure your computer is on the same network as the Z21 command station.

## Documentation

For detailed API documentation, architecture overview, and advanced usage patterns, see [DOCUMENTATION.md](DOCUMENTATION.md).

## Dependencies

- `Microsoft.Extensions.Logging.Abstractions` - Logging infrastructure
- `Tellurian.Trains.Communications.Channels` - UDP transport layer
- `Tellurian.Trains.Interfaces` - Protocol-agnostic interfaces
- `Tellurian.Trains.Protocols.XpressNet` - XpressNet protocol implementation
- `Tellurian.Trains.Protocols.LocoNet` - LocoNet protocol implementation
