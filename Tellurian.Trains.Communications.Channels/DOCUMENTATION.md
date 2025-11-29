# Tellurian.Trains.Communications.Channels Documentation

Complete API documentation for the communication channels library.

## Table of Contents

- [Dependency Injection Setup](#dependency-injection-setup)
- [Channel Architecture](#channel-architecture)
- [ICommunicationsChannel Interface](#icommunicationschannel-interface)
- [Result Types](#result-types)
- [Observer Pattern](#observer-pattern)
- [Channel Implementations](#channel-implementations)
- [Serial Port Abstractions](#serial-port-abstractions)
- [Testing](#testing)

---

## Dependency Injection Setup

The library uses constructor injection for all dependencies. Below are examples for configuring each `ICommunicationsChannel` implementation in your application's `Program.cs`.

### UDP Channel (for Z21 Command Station)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using Tellurian.Trains.Communications.Channels;

var builder = Host.CreateApplicationBuilder(args);

// Configure UDP channel for Z21
builder.Services.AddSingleton<ICommunicationsChannel>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<UdpDataChannel>>();
    var remoteEndPoint = new IPEndPoint(
        IPAddress.Parse("192.168.0.111"), 
        21105);                         
        localPort: 21105,
        remoteEndPoint,
        logger);
});
```

### UDP Channel with Configuration

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
```

#### appsettings.json:
```json
{
   "Z21": {
     "IpAddress": "192.168.0.111",
     "Port": 21105,
     "LocalPort": 21105
   }
}
```

```c
public class Z21Settings
{
    public string IpAddress { get; set; } = "192.168.0.111";
    public int Port { get; set; } = 21105;
    public int LocalPort { get; set; } = 21105;
}

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<Z21Settings>(
    builder.Configuration.GetSection("Z21"));

builder.Services.AddSingleton<ICommunicationsChannel>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<Z21Settings>>().Value;
    var logger = sp.GetRequiredService<ILogger<UdpDataChannel>>();
    var remoteEndPoint = new IPEndPoint(
        IPAddress.Parse(settings.IpAddress),
        settings.Port);
    return new UdpDataChannel(settings.LocalPort, remoteEndPoint, logger);
});
```

### Serial Channel (for LocoNet)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tellurian.Trains.Communications.Channels;
using Tellurian.Protocols.LocoNet; // Provides LocoNetFramer

var builder = Host.CreateApplicationBuilder(args);

// Register serial port adapter
builder.Services.AddSingleton<ISerialPortAdapter>(sp =>
    new SerialPortAdapter(
        portName: "COM3",
        baudRate: 57600,                    // LocoNet USB adapter standard
        parity: System.IO.Ports.Parity.None,
        dataBits: 8,
        stopBits: System.IO.Ports.StopBits.One));

// Register protocol framer
builder.Services.AddSingleton<IByteStreamFramer, LocoNetFramer>();

// Register serial channel
builder.Services.AddSingleton<ICommunicationsChannel, SerialDataChannel>();
```

### Serial Channel with Configuration

#### appsettings.json:
```json
{
  "LocoNet": {
    "PortName": "COM3",
    "BaudRate": 57600
  }
}
```

```csharp

public class LocoNetSettings
{
    public string PortName { get; set; } = "COM3";
    public int BaudRate { get; set; } = 57600;
}

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<LocoNetSettings>(
    builder.Configuration.GetSection("LocoNet"));

builder.Services.AddSingleton<ISerialPortAdapter>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<LocoNetSettings>>().Value;
    return new SerialPortAdapter(settings.PortName, settings.BaudRate);
});

builder.Services.AddSingleton<IByteStreamFramer, LocoNetFramer>();
builder.Services.AddSingleton<ICommunicationsChannel, SerialDataChannel>();
```

### Named Channels (Multiple Connections)

For applications needing multiple communication channels:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = Host.CreateApplicationBuilder(args);

// Register keyed services (.NET 8+)
builder.Services.AddKeyedSingleton<ICommunicationsChannel>("Z21", (sp, key) =>
{
    var logger = sp.GetRequiredService<ILogger<UdpDataChannel>>();
    return new UdpDataChannel(21105, new IPEndPoint(IPAddress.Parse("192.168.1.100"), 21105), logger);
});

builder.Services.AddKeyedSingleton<ICommunicationsChannel>("LocoNet", (sp, key) =>
{
    var serialPort = new SerialPortAdapter("COM3", 57600);
    var framer = sp.GetRequiredService<IByteStreamFramer>();
    var logger = sp.GetRequiredService<ILogger<SerialDataChannel>>();
    return new SerialDataChannel(serialPort, framer, logger);
});

// Inject with [FromKeyedServices("Z21")]
public class Z21Controller([FromKeyedServices("Z21")] ICommunicationsChannel channel)
{
    // Use channel
}
```

### Factory Pattern for Dynamic Creation

```csharp
public interface ICommunicationsChannelFactory
{
    ICommunicationsChannel CreateUdpChannel(string ipAddress, int port);
    ICommunicationsChannel CreateSerialChannel(string portName, int baudRate);
}

public class CommunicationsChannelFactory(ILoggerFactory loggerFactory) : ICommunicationsChannelFactory
{
    public ICommunicationsChannel CreateUdpChannel(string ipAddress, int port)
    {
        var logger = loggerFactory.CreateLogger<UdpDataChannel>();
        var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        return new UdpDataChannel(port, endpoint, logger);
    }

    public ICommunicationsChannel CreateSerialChannel(string portName, int baudRate)
    {
        var serialPort = new SerialPortAdapter(portName, baudRate);
        var framer = new LocoNetFramer();
        var logger = loggerFactory.CreateLogger<SerialDataChannel>();
        return new SerialDataChannel(serialPort, framer, logger);
    }
}

// Register factory
builder.Services.AddSingleton<ICommunicationsChannelFactory, CommunicationsChannelFactory>();
```

---

## Channel Architecture

### Lifecycle Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    ICommunicationsChannel                       │
├─────────────────────────────────────────────────────────────────┤
│  1. Create channel instance                                     │
│  2. Subscribe observer(s) via Subscribe()                       │
│  3. Start receive loop via StartReceiveAsync()                  │
│  4. Send data via SendAsync()                                   │
│  5. Receive notifications via IObserver.OnNext()                │
│  6. Cancel via CancellationToken                                │
│  7. Dispose channel via DisposeAsync()                          │
└─────────────────────────────────────────────────────────────────┘
```

### Data Flow

```
Outbound (Send):
┌─────────┐    SendAsync(byte[])    ┌─────────────┐    Transport    ┌──────────┐
│  Caller │ ───────────────────────>│   Channel   │ ───────────────>│  Device  │
└─────────┘  <─ CommunicationResult └─────────────┘                 └──────────┘

Inbound (Receive):
┌──────────┐    Transport    ┌─────────────┐    OnNext(result)    ┌───────────┐
│  Device  │ ───────────────>│   Channel   │ ───────────────────> │  Observer │
└──────────┘                 └─────────────┘                      └───────────┘
                                   │
                                   │ OnNext(result)
                                   v
                             ┌───────────┐
                             │ Observer 2│
                             └───────────┘
```

---

## ICommunicationsChannel Interface

```csharp
public interface ICommunicationsChannel : IObservable<CommunicationResult>
{
    Task<CommunicationResult> SendAsync(byte[] data, CancellationToken cancellationToken = default);
    Task StartReceiveAsync(CancellationToken cancellationToken = default);
}
```

### SendAsync

Sends raw bytes to the remote endpoint.

**Parameters:**
- `data`: Byte array to send (null or empty returns `NoOperationResult`)
- `cancellationToken`: Token to cancel the operation

**Returns:**
- `SuccessResult`: Data was sent successfully
- `FailureResult`: An error occurred (contains exception)
- `NoOperationResult`: Data was null or empty

**Example:**
```csharp
var result = await channel.SendAsync(new byte[] { 0x04, 0x00, 0x10, 0x00 }, cts.Token);

switch (result)
{
    case SuccessResult success:
        Console.WriteLine($"Sent {success.Length} bytes");
        break;
    case FailureResult failure:
        Console.WriteLine($"Send failed: {failure.Exception.Message}");
        break;
    case NoOperationResult:
        Console.WriteLine("Nothing to send");
        break;
}
```

### StartReceiveAsync

Starts the background receive loop. Returns immediately after starting.

**Parameters:**
- `cancellationToken`: Token to stop the receive loop

**Behavior:**
1. Starts a background task that continuously reads from the transport
2. Returns immediately (non-blocking)
3. Received data is delivered via `IObserver<CommunicationResult>.OnNext()`
4. Loop continues until cancellation token is triggered or channel is disposed

**Example:**
```csharp
var cts = new CancellationTokenSource();

// Start receiving in background
await channel.StartReceiveAsync(cts.Token);

// Do other work...

// Stop receiving
cts.Cancel();
```

### Subscribe (from IObservable)

Registers an observer to receive notifications.

**Returns:** `IDisposable` - Dispose to unsubscribe

**Example:**
```csharp
var subscription = channel.Subscribe(myObserver);

// Later, stop receiving notifications
subscription.Dispose();
```

---

## Result Types

The library uses a Result Type pattern instead of exceptions for expected failure modes.

### CommunicationResult (Abstract Base)

```csharp
public abstract class CommunicationResult
{
    public virtual bool IsSuccess { get; } = false;
    public virtual int Length { get; } = 0;
    public DateTimeOffset Timestamp { get; }

    // Factory methods
    public static CommunicationResult Success(byte[] data, string remoteEndpointName, string protocolName);
    public static CommunicationResult Failure(Exception ex);
    public static CommunicationResult NoOperation();
}
```

### SuccessResult

Represents a successful send or receive operation.

```csharp
public sealed class SuccessResult : CommunicationResult
{
    public override bool IsSuccess => true;
    public override int Length => _Data.Length;
    public byte[] Data();                    // The payload bytes
    public string RemoteEndpointName { get; } // e.g., "192.168.1.100:21105"
    public string ProtocolName { get; }       // e.g., "UDP", "LocoNet"
}
```

### FailureResult

Represents an operation failure.

```csharp
public sealed class FailureResult : CommunicationResult
{
    public Exception Exception { get; }
}
```

### NoOperationResult

Represents a no-op (e.g., sending empty data).

```csharp
public sealed class NoOperationResult : CommunicationResult { }
```

### Pattern Matching Example

```csharp
void HandleResult(CommunicationResult result)
{
    switch (result)
    {
        case SuccessResult { Length: > 0 } success:
            ProcessData(success.Data());
            break;
        case FailureResult { Exception: SocketException se }:
            HandleNetworkError(se);
            break;
        case FailureResult failure:
            LogError(failure.Exception);
            break;
        case NoOperationResult:
            // Nothing to do
            break;
    }
}
```

---

## Observer Pattern

### How It Works

The channel implements `IObservable<CommunicationResult>`, allowing multiple observers to receive notifications of incoming data.

```
┌─────────────────────────────────────────────────────────────────┐
│                         Channel                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              Observers<CommunicationResult>             │    │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐                 │    │
│  │  │Observer 1│ │Observer 2│ │Observer 3│ ...             │    │
│  │  └──────────┘ └──────────┘ └──────────┘                 │    │
│  └─────────────────────────────────────────────────────────┘    │
│                          │                                      │
│                          │ Notify(result)                       │
│                          v                                      │
│                    All observers receive OnNext(result)         │
└─────────────────────────────────────────────────────────────────┘
```

### Implementing IObserver

```csharp
public class DataReceiver : IObserver<CommunicationResult>
{
    public void OnNext(CommunicationResult result)
    {
        if (result is SuccessResult success)
        {
            // Process received data
            var data = success.Data();
            Console.WriteLine($"Received {data.Length} bytes at {result.Timestamp}");
        }
    }

    public void OnError(Exception error)
    {
        // Handle unrecoverable channel error
        Console.WriteLine($"Channel error: {error.Message}");
    }

    public void OnCompleted()
    {
        // Channel has been disposed
        Console.WriteLine("Channel closed");
    }
}
```

### Using ActionObserver

For functional-style programming, use the internal `ActionObserver<T>` pattern:

```csharp
// This pattern is used internally by higher-level adapters
var observer = new ActionObserver<CommunicationResult>(
    onNext: result => ProcessResult(result),
    onError: ex => LogError(ex),
    onCompleted: () => Cleanup()
);

channel.Subscribe(observer);
```

### Multiple Observers

```csharp
// Logger observer
var loggerObserver = new LoggingObserver(logger);
var logSub = channel.Subscribe(loggerObserver);

// Statistics observer
var statsObserver = new StatisticsObserver();
var statsSub = channel.Subscribe(statsObserver);

// Data processor observer
var processorObserver = new DataProcessorObserver();
var processorSub = channel.Subscribe(processorObserver);

// All three receive every notification
await channel.StartReceiveAsync(cts.Token);

// Unsubscribe individually
processorSub.Dispose();  // Stop processing, keep logging and stats
```

### Observers<T> Collection

The `Observers<T>` class manages subscriptions internally:

```csharp
public sealed class Observers<T> where T : class
{
    public int Count { get; }                           // Number of subscribers
    public void Notify(T notification);                 // Broadcast to all
    public void Notify(T[] notifications);              // Broadcast multiple
    public void Completed();                            // Signal completion
    public void Error(Exception ex);                    // Signal error
    public IDisposable Subscribe(IObserver<T> observer); // Add subscriber
}
```

### Thread Safety Considerations

- Observers are notified on background threads (thread pool)
- Observer implementations must be thread-safe
- No locks are held during callbacks (subscribe/unsubscribe during notification is safe)

---

## Channel Implementations

### UdpDataChannel

For UDP-based communication (e.g., Z21 command station).

```csharp
public sealed class UdpDataChannel : ICommunicationsChannel, IAsyncDisposable, IDisposable
{
    public UdpDataChannel(int localPort, IPEndPoint remoteEndPoint, ILogger<UdpDataChannel> logger);
}
```

**Constructor Parameters:**
- `localPort`: UDP port to listen on for incoming packets
- `remoteEndPoint`: Target IP address and port for outgoing packets
- `logger`: Logger for diagnostic output

**Characteristics:**
- Uses `System.Net.Sockets.UdpClient` internally
- Bidirectional communication on same port
- Packets are atomic (no framing needed)
- Supports both sync and async disposal

**Example:**
```csharp
var logger = loggerFactory.CreateLogger<UdpDataChannel>();
var remote = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 21105);

await using var channel = new UdpDataChannel(21105, remote, logger);

channel.Subscribe(myObserver);
await channel.StartReceiveAsync(cts.Token);

// Z21 get serial number command
await channel.SendAsync(new byte[] { 0x04, 0x00, 0x10, 0x00 }, cts.Token);
```

### SerialDataChannel

For serial port communication (e.g., LocoNet, XpressNet).

```csharp
public sealed class SerialDataChannel : ICommunicationsChannel, IAsyncDisposable, IDisposable
{
    public SerialDataChannel(
        ISerialPortAdapter serialPort,
        IByteStreamFramer framer,
        ILogger<SerialDataChannel> logger);
}
```

**Constructor Parameters:**
- `serialPort`: Serial port adapter (abstraction over `System.IO.Ports.SerialPort`)
- `framer`: Protocol-specific message framer (assembles complete messages)
- `logger`: Logger for diagnostic output

**Characteristics:**
- Auto-opens port if not already open
- Uses framer to assemble complete protocol messages from byte stream
- Supports different protocols via framer injection

**Example:**
```csharp
var serialPort = new SerialPortAdapter("COM3", 57600);
var framer = new LocoNetFramer();
var logger = loggerFactory.CreateLogger<SerialDataChannel>();

await using var channel = new SerialDataChannel(serialPort, framer, logger);

channel.Subscribe(myObserver);
await channel.StartReceiveAsync(cts.Token);

// Send LocoNet message
await channel.SendAsync(new byte[] { 0xB5, 0x01, 0x02, 0xF2 }, cts.Token);
```

---

## Serial Port Abstractions

### ISerialPortAdapter

Abstracts the serial port for testability.

```csharp
public interface ISerialPortAdapter : IDisposable
{
    bool IsOpen { get; }
    string PortName { get; }
    int ReadTimeout { get; set; }

    void Open();
    void Close();
    Task WriteAsync(byte[] data, CancellationToken cancellationToken = default);
    ValueTask<int> ReadByteAsync(CancellationToken cancellationToken = default);
}
```

**ReadByteAsync Return Values:**
- `0-255`: The byte value read
- `-1`: Read timeout (no data available)

### SerialPortAdapter

Production implementation wrapping `System.IO.Ports.SerialPort`.

```csharp
public sealed class SerialPortAdapter : ISerialPortAdapter
{
    public SerialPortAdapter(
        string portName,
        int baudRate = 57600,
        Parity parity = Parity.None,
        int dataBits = 8,
        StopBits stopBits = StopBits.One);
}
```

**Default Baud Rate:** 57600 (standard for LocoNet USB adapters)

### IByteStreamFramer

Assembles complete protocol messages from a byte stream.

```csharp
public delegate ValueTask<int> ReadByteDelegate(CancellationToken cancellationToken);

public interface IByteStreamFramer
{
    string ProtocolName { get; }
    Task<byte[]?> ReadMessageAsync(ReadByteDelegate readByte, CancellationToken cancellationToken = default);
}
```

**ReadMessageAsync Return Values:**
- `byte[]`: Complete message assembled from stream
- `null`: Timeout, cancellation, or sync error requiring resync

**Protocol Name:** Identifier used in result logging (e.g., "LocoNet", "XpressNet")

**Available Implementations:**
- `LocoNetFramer` (in `Tellurian.Protocols.LocoNet`)
- `XpressNetFramer` (in `Tellurian.Trains.Protocols.XpressNet`)

---

## Testing

### MockSerialPortAdapter

Test double for serial port testing without hardware.

```csharp
public sealed class MockSerialPortAdapter : ISerialPortAdapter
{
    public MockSerialPortAdapter(string portName);

    public void EnqueueBytes(params byte[] bytes);
    public void EnqueueMessageWithChecksum(params byte[] messageWithoutChecksum);
    public IReadOnlyList<byte[]> WriteHistory { get; }
    public int WriteCount { get; }
    public void Clear();
}
```

**Example Test:**
```csharp
[TestMethod]
public async Task ReceivesLocoNetMessage()
{
    // Arrange
    var mock = new MockSerialPortAdapter("MOCK");
    mock.Open();
    mock.EnqueueMessageWithChecksum(0xB5, 0x01, 0x02); // Adds checksum automatically

    var framer = new LocoNetFramer();
    var logger = NullLogger<SerialDataChannel>.Instance;
    await using var channel = new SerialDataChannel(mock, framer, logger);

    var received = new List<byte[]>();
    channel.Subscribe(new ActionObserver<CommunicationResult>(
        result => { if (result is SuccessResult s) received.Add(s.Data()); },
        _ => { },
        () => { }));

    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
    await channel.StartReceiveAsync(cts.Token);

    // Assert
    await Task.Delay(100);
    Assert.AreEqual(1, received.Count);
    Assert.AreEqual(4, received[0].Length); // 3 bytes + checksum
}
```

### UDP Testing with Loopback

```csharp
[TestMethod]
public async Task UdpChannelSendsAndReceives()
{
    // Arrange
    var logger = NullLogger<UdpDataChannel>.Instance;
    var endpoint1 = new IPEndPoint(IPAddress.Loopback, 9901);
    var endpoint2 = new IPEndPoint(IPAddress.Loopback, 9902);

    await using var channel1 = new UdpDataChannel(9901, endpoint2, logger);
    await using var channel2 = new UdpDataChannel(9902, endpoint1, logger);

    var received = new TaskCompletionSource<byte[]>();
    channel2.Subscribe(new ActionObserver<CommunicationResult>(
        result => { if (result is SuccessResult s) received.TrySetResult(s.Data()); },
        _ => { },
        () => { }));

    // Act
    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    await channel2.StartReceiveAsync(cts.Token);
    await channel1.SendAsync(new byte[] { 1, 2, 3 }, cts.Token);

    // Assert
    var data = await received.Task;
    CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, data);
}
```

### Test Observer Helper

```csharp
public class TestObserver<T> : IObserver<T>
{
    private readonly List<T> _notifications = [];
    public IReadOnlyList<T> Notifications => _notifications;
    public Exception? Error { get; private set; }
    public bool IsCompleted { get; private set; }

    public void OnNext(T value) => _notifications.Add(value);
    public void OnError(Exception error) => Error = error;
    public void OnCompleted() => IsCompleted = true;
}
```

---

## Logging

All channels use `ILogger<T>` for structured logging.

### Log Levels Used

| Level | Purpose |
|-------|---------|
| Debug | Payload tracing, timing info |
| Information | Connection state changes |
| Warning | Recoverable issues (e.g., timeout, resync) |
| Error | Operation failures |

### Enabling Debug Logging

```csharp
builder.Logging.AddFilter("Tellurian.Trains.Communications.Channels", LogLevel.Debug);
```

### Example Log Output

```
dbug: UdpDataChannel[0] Sending 4 bytes to 192.168.1.100:21105
dbug: UdpDataChannel[0] Received 8 bytes from 192.168.1.100:21105
warn: SerialDataChannel[0] Port COM3 was closed, reopening
info: SerialDataChannel[0] Started receiving on COM3
```
