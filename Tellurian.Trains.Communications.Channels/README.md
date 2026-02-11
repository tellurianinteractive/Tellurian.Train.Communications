# Tellurian.Trains.Communications.Channels

A protocol-agnostic transport layer library for model train control systems. Provides UDP, serial port, TCP, and UDP multicast communication with async/await patterns and observer-based notifications.

## Features

- **Protocol-agnostic**: Works with any byte-based protocol (Z21, LocoNet, XpressNet)
- **Async-first**: All I/O operations use async/await with cancellation support
- **Observer pattern**: Standard `IObservable<T>` for receiving data notifications
- **Result types**: Explicit success/failure handling without exceptions
- **Testable**: Interface-based design with mock implementations

## Quick Start

### UDP Channel (Z21 Command Station)

```csharp
using Microsoft.Extensions.Logging;
using Tellurian.Trains.Communications.Channels;
using System.Net;

// Create channel
var logger = loggerFactory.CreateLogger<UdpDataChannel>();
var remoteEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 21105);
await using var channel = new UdpDataChannel(localPort: 21105, remoteEndPoint, logger);

// Subscribe to received data
var subscription = channel.Subscribe(new MyObserver());

// Start background receive loop
var cts = new CancellationTokenSource();
await channel.StartReceiveAsync(cts.Token);

// Send data
var result = await channel.SendAsync(new byte[] { 0x04, 0x00, 0x10, 0x00 }, cts.Token);
if (result.IsSuccess)
{
    Console.WriteLine("Sent successfully");
}

// Cleanup
cts.Cancel();
subscription.Dispose();
```

### Serial Channel (LocoNet)

```csharp
using Microsoft.Extensions.Logging;
using Tellurian.Trains.Communications.Channels;

// Create serial port adapter
var serialPort = new SerialPortAdapter("COM3", baudRate: 57600);

// Create channel with protocol framer
var framer = new LocoNetFramer(); // From Tellurian.Protocols.LocoNet
var logger = loggerFactory.CreateLogger<SerialDataChannel>();
await using var channel = new SerialDataChannel(serialPort, framer, logger);

// Subscribe and start receiving
var subscription = channel.Subscribe(new MyObserver());
var cts = new CancellationTokenSource();
await channel.StartReceiveAsync(cts.Token);

// Send LocoNet message
await channel.SendAsync(new byte[] { 0xB5, 0x01, 0x02, 0xF2 }, cts.Token);
```

### TCP Channel (LoconetOverTcp)

```csharp
using Tellurian.Trains.Communications.Channels;

// Connect to a LoconetOverTcp server (LbServer, JMRI, Rocrail)
var stream = new TcpStreamAdapter("192.168.1.100", 1234);
var logger = loggerFactory.CreateLogger<TcpLocoNetChannel>();
await using var channel = new TcpLocoNetChannel(stream, logger);

var subscription = channel.Subscribe(new MyObserver());
var cts = new CancellationTokenSource();
await channel.StartReceiveAsync(cts.Token);
await channel.SendAsync(new byte[] { 0xB5, 0x01, 0x02, 0xF2 }, cts.Token);
```

### UDP Multicast Channel (LocoNet over UDP)

```csharp
using System.Net;
using Tellurian.Trains.Communications.Channels;

// loconetd (Glenn Butcher): multicast 225.0.0.2, listen 4501, send to gateway on 4500
var adapter = new UdpLocoNetAdapter(
    multicastGroup: IPAddress.Parse("225.0.0.2"),
    listenPort: 4501,
    sendEndpoint: new IPEndPoint(IPAddress.Parse("192.168.1.50"), 4500));
var logger = loggerFactory.CreateLogger<UdpLocoNetChannel>();
await using var channel = new UdpLocoNetChannel(adapter, logger);

var subscription = channel.Subscribe(new MyObserver());
var cts = new CancellationTokenSource();
await channel.StartReceiveAsync(cts.Token);
await channel.SendAsync(new byte[] { 0xB5, 0x01, 0x02, 0xF2 }, cts.Token);
```

### Handling Received Data

```csharp
public class MyObserver : IObserver<CommunicationResult>
{
    public void OnNext(CommunicationResult result)
    {
        if (result is SuccessResult success)
        {
            byte[] data = success.Data();
            Console.WriteLine($"Received {data.Length} bytes from {success.RemoteEndpointName}");
        }
        else if (result is FailureResult failure)
        {
            Console.WriteLine($"Error: {failure.Exception.Message}");
        }
    }

    public void OnError(Exception error) => Console.WriteLine($"Channel error: {error.Message}");
    public void OnCompleted() => Console.WriteLine("Channel closed");
}
```

## Installation

Add a reference to the project or NuGet package:

```xml
<PackageReference Include="Tellurian.Trains.Communications.Channels" />
```

## Documentation

See [DOCUMENTATION.md](DOCUMENTATION.md) for complete API documentation including:

- Dependency injection configuration
- Channel architecture and lifecycle
- Observer pattern implementation
- Result type handling
- Testing patterns

## Requirements

- .NET 10.0 or later
- For serial ports: Windows, Linux, or macOS with serial port support, for example with a USB-to-serial adapter.
