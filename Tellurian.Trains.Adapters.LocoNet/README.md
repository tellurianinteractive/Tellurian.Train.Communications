# Tellurian.Trains.Adapters.LocoNet

A .NET adapter for LocoNet-based command stations via serial port, providing a high-level API for locomotive control, accessory switching, and decoder programming.

## Features

- **Locomotive Control** (`ILoco`): Speed, direction, and function control (F0-F12)
- **Accessory Control** (`IAccessory`): Set and query accessory states
- **Switch Control** (`ISwitch`): Convenient throw/close/off operations for switches
- **Decoder Programming** (`IDecoder`): CV read/write operations on the programming track

## Usage

```csharp
// Create serial port adapter
var serialPort = new SerialPortAdapter("COM3", 57600);

// Create LocoNet framer
var framer = new LocoNetFramer();

// Create communication channel
var channel = new SerialDataChannel(serialPort, framer, logger);

// Create adapter
await using var adapter = new Adapter(channel, logger);

// Start receiving
await adapter.StartReceiveAsync();

// Control a locomotive
var address = Address.From(999);
var drive = new Drive
{
    Direction = Direction.Forward,
    Speed = Speed.Set126(50)
};
await adapter.DriveAsync(address, drive);

// Control a switch
var switchAddress = Accessories.Address.From(1);
await adapter.SetThrownAsync(switchAddress);
```

## Slot Management

LocoNet uses a slot-based architecture where each locomotive is assigned to a slot (0-119). This adapter handles slot management automatically:

1. When you control a locomotive, the adapter requests a slot for the address
2. The slot assignment is cached for subsequent operations
3. Slots are automatically managed for the lifetime of the adapter

## Notifications

Subscribe to receive state change notifications:

```csharp
adapter.Subscribe(new NotificationObserver());

class NotificationObserver : IObserver<Notification>
{
    public void OnNext(Notification notification)
    {
        // Handle notification
    }

    public void OnError(Exception error) { }
    public void OnCompleted() { }
}
```

## Requirements

- LocoNet interface connected via serial port
- .NET 10.0 or later
