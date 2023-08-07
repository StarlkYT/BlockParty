using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Starlk.Console.Client;

namespace Starlk.Console.Hosting;

internal static class GraniteServerHostBuilderExtensions
{
    public static IHostBuilder AddGraniteServer(this IHostBuilder builder, Action<GraniteServerOptions> configure)
    {
        var options = new GraniteServerOptions();
        configure(options);

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<Lobby>();

            services.AddTransient<Func<GraniteServer, ConnectionContext, GraniteClient>>(
                provider => (server, context) =>
                    new GraniteClient(
                        provider.GetRequiredService<ILogger<GraniteClient>>(),
                        server,
                        context));

            services.AddTransient<GraniteServer>(provider =>
                new GraniteServer(
                    options,
                    provider.GetRequiredService<ILoggerFactory>(),
                    provider.GetRequiredService<Func<GraniteServer, ConnectionContext, GraniteClient>>(),
                    provider.GetRequiredService<Func<GraniteServer, Session>>(),
                    provider.GetRequiredService<Lobby>()));

            services.AddTransient<Func<GraniteServer, Session>>(provider =>
                server => new Session(provider.GetRequiredService<ILogger<Session>>(), server));

            services.AddHostedService<GraniteServerHostedService>();
        });

        return builder;
    }
}