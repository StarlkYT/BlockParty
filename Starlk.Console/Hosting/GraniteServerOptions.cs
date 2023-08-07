using System.Net;
using Starlk.Console.Components;

namespace Starlk.Console.Hosting;

internal sealed class GraniteServerOptions
{
    public IPAddress Address { get; set; } = IPAddress.Loopback;

    public int Port { get; set; } = 25565;

    public Chat Description { get; set; } = Chat.Create("A Minecraft Server");
}