using System.Net;
using System.Net.Sockets;

namespace Tellurian.Trains.Communications.Channels.Tests;

public sealed class UdpState
{
    public UdpClient? Client { get; set; }
    public IPEndPoint? Source { get; set; }
}
