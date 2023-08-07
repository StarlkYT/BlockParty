using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starlk.Console.Client;
using Starlk.Console.Components;
using Starlk.Console.Components.Blocks;
using Starlk.Console.Hosting;
using Starlk.Console.Networking;
using Starlk.Console.Networking.Packets.Play;

namespace Starlk.Console;

internal sealed class GraniteServer
{
    public ServerStatus Status => ServerStatus.Create(
        "Starlk",
        47,
        10,
        clients.Count,
        options.Description);

    public Session Session { get; }

    public Lobby Lobby { get; }

    private IConnectionListener? listener;

    private readonly SocketTransportFactory transportFactory;
    private readonly IPEndPoint endpoint;
    private readonly ILogger<GraniteServer> logger;
    private readonly List<GraniteClient> clients;
    private readonly GraniteServerOptions options;
    private readonly Func<GraniteServer, ConnectionContext, GraniteClient> clientFactory;

    public GraniteServer(
        GraniteServerOptions options,
        ILoggerFactory loggerFactory,
        Func<GraniteServer, ConnectionContext, GraniteClient> clientFactory,
        Func<GraniteServer, Session> sessionFactory,
        Lobby lobby)
    {
        transportFactory = new SocketTransportFactory(
            Options.Create(new SocketTransportOptions()),
            loggerFactory);

        endpoint = new IPEndPoint(options.Address, options.Port);
        logger = loggerFactory.CreateLogger<GraniteServer>();
        clients = new List<GraniteClient>(10);

        this.options = options;
        this.clientFactory = clientFactory;

        Session = sessionFactory(this);
        Lobby = lobby;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Lobby.LoadAsync();

        listener = await transportFactory.BindAsync(endpoint, cancellationToken);
        logger.LogInformation("Listening at \"{Endpoint}\"", endpoint);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var connection = await listener.AcceptAsync(cancellationToken);

                if (connection is null)
                {
                    logger.LogWarning("No longer accepting connections");
                    break;
                }

                var client = clientFactory(this, connection);
                _ = Task.Run(client.ConnectAsync, CancellationToken.None);
                logger.LogDebug("Accepted connection");
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    public async Task AddClientAsync(GraniteClient client)
    {
        clients.Add(client);

        await UpdatePlayerListItemAsync();
        await SpawnPlayerAsync(client);
        await UpdatePlayerLookAsync(client.Player!);

        if (clients.Count > 0)
        {
            _ = Task.Run(async () => await Session.StartCountDownAsync());
        }
    }

    public async Task RemoveClientAsync(GraniteClient client)
    {
        clients.Remove(client);

        if (clients.Count == 0)
        {
            await Session.CancelAsync();
        }
    }

    public async Task DestroyEntityAsync(int id)
    {
        foreach (var client in clients)
        {
            await client.Connection.WriteAsync(new DestroyEntitiesPacket()
            {
                Ids = new[] { id }
            });
        }
    }

    public async Task UpdatePlayerLookAsync(MinecraftPlayer player)
    {
        foreach (var client in clients.Where(client => client.Player!.Username != player.Username))
        {
            var yaw = (byte) (player.Yaw / 256 * 360);
            var pitch = (byte) (player.Pitch / 256 * 360);

            await client.Connection.WriteAsync(new EntityLookPacket
            {
                Id = player.Id,
                Yaw = yaw,
                Pitch = pitch,
                OnGround = player.OnGround
            });
        }
    }

    public async Task PlaySoundEffectAsync(string name)
    {
        await BroadcastToPlayersAsync(async player => await player.PlaySoundEffectAsync(name));
    }

    public async Task BuildRandomFloorAsync()
    {
        var blocks = new BlockChange[256];

        for (byte x = 0; x < 16; x++)
        {
            for (byte z = 0; z < 16; z++)
            {
                var index = x + z * 16;
                blocks[index] = new BlockChange(x, 0, z, new Block(159, Random.Shared.Next(15)));
            }
        }

        foreach (var client in clients)
        {
            await Lobby.UpdateFloorAsync(blocks, client);
            await client.Player!.PlaySoundEffectAsync(SoundEffect.RandomLevelUp);
        }
    }

    public async Task DestroyFloor()
    {
        foreach (var client in clients)
        {
            await Lobby.DestroyFloorAsync(client);
            await client.Player!.PlaySoundEffectAsync(SoundEffect.NoteHat);
        }
    }

    private async Task SpawnPlayerAsync(GraniteClient newClient)
    {
        foreach (var client in clients.Where(
                     client => client.Player!.Username != newClient.Player!.Username))
        {
            await client.Connection.WriteAsync(new SpawnPlayerPacket
            {
                Id = newClient.Player!.Id,
                Guid = newClient.Player.Guid,
                X = newClient.Player.X,
                Y = newClient.Player.Y,
                Z = newClient.Player.Z,
                Yaw = newClient.Player.Yaw,
                Pitch = newClient.Player.Pitch,
                CurrentItem = 0
            });

            await newClient.Connection.WriteAsync(new SpawnPlayerPacket
            {
                Id = client.Player!.Id,
                Guid = client.Player.Guid,
                X = client.Player.X,
                Y = client.Player.Y,
                Z = client.Player.Z,
                Yaw = client.Player.Yaw,
                Pitch = client.Player.Pitch,
                CurrentItem = 0
            });
        }
    }

    public async Task UpdatePlayerPositionAsync(GraniteClient playerClient, sbyte x, sbyte y, sbyte z, bool onGround)
    {
        var player = playerClient.Player!;
        foreach (var client in clients.Where(client => client.Player!.Username != player.Username))
        {
            await client.Connection.WriteAsync(new EntityRelativeMovePacket()
            {
                Id = player.Id,
                X = x,
                Y = y,
                Z = z,
                OnGround = onGround
            });
        }
    }

    public async Task UpdateSlotAsync(bool clear = false)
    {
        var block = (byte) (clear ? 0 : 159);
        var metadata = (byte) (clear ? 0 : Lobby.SampledBlock!.Block.Metadata);

        foreach (var client in clients)
        {
            await client.Connection.WriteAsync(new HeldItemChangePacket()
            {
                Slot = 4
            });

            await client.Connection.WriteAsync(new SetSlotPacket()
            {
                Block = block,
                Metadata = metadata
            });
        }
    }

    // public async Task ClearSlotAsync()
    // {
    //     foreach (var client in clients)
    //     {
    //         await client.Connection.WriteAsync(new HeldItemChangePacket()
    //         {
    //             Slot = 4
    //         });
    //
    //         await client.Connection.WriteAsync(new SetSlotPacket()
    //         {
    //             Block = 0,
    //             Metadata = 0
    //         });
    //     }
    // }

    public async Task UpdatePlayerListItemAsync(IAction? action = null)
    {
        action ??= new AddPlayerAction()
        {
            Players = clients.Select(predicate => predicate.Player!).ToArray()
        };

        var packet = new PlayerListItemPacket()
        {
            Action = action
        };

        await BroadcastToPlayersAsync(async player => await player.UpdatePlayerListItemAsync(packet));
    }

    public async Task SendChatMessageAsync(Chat chat, ChatPosition position = ChatPosition.ChatBox)
    {
        await BroadcastToPlayersAsync(async player => await player.SendChatMessageAsync(chat, position));
    }

    public async Task KickAllAsync(Chat reason)
    {
        while (clients.Count > 0)
        {
            try
            {
                foreach (var client in clients)
                {
                    if (client.Player is { } player)
                    {
                        await player.KickAsync(reason);
                    }
                    else
                    {
                        await client.DisconnectAsync();
                    }
                }
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    private async Task BroadcastToPlayersAsync(Func<MinecraftPlayer, Task> action)
    {
        foreach (var client in clients)
        {
            if (client.Player is { } player)
            {
                await action(player);
            }
        }
    }

    public async Task StopAsync()
    {
        await KickAllAsync(Chat.Create("Server closed", color: "red"));
        await listener!.UnbindAsync();
        await listener.DisposeAsync();

        Debug.Assert(clients.Count is 0);
        logger.LogInformation("Stopped listening");
    }
}