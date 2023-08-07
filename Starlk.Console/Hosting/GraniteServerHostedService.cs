using Microsoft.Extensions.Hosting;

namespace Starlk.Console.Hosting;

internal sealed class GraniteServerHostedService : IHostedService
{
    private readonly GraniteServer server;

    public GraniteServerHostedService(GraniteServer server)
    {
        this.server = server;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await server.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await server.StopAsync();
    }
}