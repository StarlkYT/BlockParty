using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Starlk.Console.Components;
using Starlk.Console.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureLogging(configure => configure.SetMinimumLevel(LogLevel.Debug))
    .AddGraniteServer(options =>
    {
        options.Address = IPAddress.Any;
        options.Description = Chat.Create("Starlk's server", color: "blue");
    })
    .Build()
    .RunAsync();