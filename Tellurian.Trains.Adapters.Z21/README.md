# Tellurian.Trains.Adapters.Z21

A .NET adapter for the **Roco/Fleischmann Z21** digital command station, providing a high-level API for locomotive control, accessory switching, and system monitoring over UDP.

## Features

- **Locomotive Control**: Drive, emergency stop, function control, and state query via `ILoco`
- **Protocol Support**: Encapsulates both XpressNet and LocoNet protocols
- **Real-time Notifications**: Observer-based notification system for state changes
- **System Monitoring**: Track voltage, current, temperature, and status
- **Detector Support**: Occupancy, transponder, and RailCom notifications via Z21 LocoNet detector API (LAN_LOCONET_DETECTOR 0xA4) and CAN detector API (LAN_CAN_DETECTOR 0xC4) for Z21 10808 modules
- **Broadcast Filtering**: Configurable subscription to specific event types
- **Accessory Gateway Mode**: Accessory commands (`IAccessory`, `ITurnout`) are sent as wrapped LocoNet via `LAN_LOCONET_FROM_LAN` by default so that state-change feedback flows through the Z21's LocoNet forwarding (subscribe `BroadcastSubjects.LocoNetTurnouts`) and is decoded into protocol-agnostic `AccessoryNotification`. Opt out to plain XpressNet for DCC-only decoders by passing `useLocoNetForAccessories: false` to the constructor.

## Quick Start

### Dependency Injection Setup

Add the Z21 adapter to your application in `Program.cs`:

```csharp
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tellurian.Trains.Adapters.Z21;
using Tellurian.Trains.Communications.Channels;
using Tellurian.Trains.Communications.Interfaces.Locos;

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

// Register the Z21 adapter with an initial broadcast subscription.
// Pick only the subjects your application actually needs — the Z21 spec
// warns that broad subscriptions (e.g. all locos, all switches) can generate
// considerable network traffic, especially over Wi-Fi.
services.AddSingleton(sp => new Adapter(
    sp.GetRequiredService<ICommunicationsChannel>(),
    sp.GetRequiredService<ILogger<Adapter>>(),
    BroadcastSubjects.RunningAndSwitching | BroadcastSubjects.SystemStateChanges));

// Register ILoco interface (the Adapter implements it)
services.AddSingleton<ILoco>(sp => sp.GetRequiredService<Adapter>());

var provider = services.BuildServiceProvider();
```

### Basic Usage

```csharp
using Tellurian.Trains.Adapters.Z21;
using Tellurian.Trains.Communications.Interfaces.Locos;

// Get the adapter from DI
var adapter = provider.GetRequiredService<Adapter>();

// Subscribe to notifications
adapter.Subscribe(new MyNotificationObserver());

// Start receiving notifications. The adapter automatically applies the
// BroadcastSubjects passed to its constructor to the Z21 — and will re-apply
// them on every (re)connect, because the Z21 forgets broadcast flags when
// the client reconnects.
await adapter.StartReceiveAsync();

// Change the subscription set at runtime if needed.
// The Z21 protocol only supports "set absolute" — the adapter tracks the
// current set so you can add/remove individual subjects conveniently.
await adapter.AddSubscriptionsAsync(BroadcastSubjects.LocoNetTurnouts);
await adapter.RemoveSubscriptionsAsync(BroadcastSubjects.SystemStateChanges);
// Or replace the whole set in one call:
await adapter.SubscribeAsync(BroadcastSubjects.LocoNetTurnouts);

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
using Tellurian.Trains.Communications.Interfaces;

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

## Accessory Addressing

This library uses **1-based user addresses** (1-2048) for accessories/turnouts, which matches how users typically configure their systems. The protocol layer automatically converts to 0-based wire addresses.

### Z21 Address Shift (+4)
There is a historical ambiguity in DCC where Roco/Z21 numbers accessory modules starting from 0, while other manufacturers start from 1. This creates a 4-address shift between systems.

The Z21 has a hardware setting **"DCC-Weichenadressverschiebung +4"** to compensate. If your turnouts are off by 4 addresses compared to another system, enable this option in Z21 Maintenance.

## Documentation

For detailed API documentation, architecture overview, and advanced usage patterns, see [DOCUMENTATION.md](DOCUMENTATION.md).

## Dependencies

- `Microsoft.Extensions.Logging.Abstractions` - Logging infrastructure
- `Tellurian.Trains.Communications.Channels` - UDP transport layer
- `Tellurian.Trains.Communications.Interfaces` - Protocol-agnostic interfaces
- `Tellurian.Trains.Protocols.XpressNet` - XpressNet protocol implementation
- `Tellurian.Trains.Protocols.LocoNet` - LocoNet protocol implementation
