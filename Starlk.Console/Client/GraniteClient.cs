using System.Diagnostics;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Starlk.Console.Components;
using Starlk.Console.Networking;
using Starlk.Console.Networking.Packets;
using Starlk.Console.Networking.Packets.Handshaking;
using Starlk.Console.Networking.Packets.Login;
using Starlk.Console.Networking.Packets.Play;
using Starlk.Console.Networking.Packets.Status;

namespace Starlk.Console.Client;

internal sealed class GraniteClient
{
    public MinecraftPlayer? Player { get; private set; }

    public ConnectionContext Connection { get; }

    private GraniteClientState state;

    private readonly ILogger<GraniteClient> logger;
    private readonly GraniteServer server;
    private readonly Timer keepAliveTimer;

    public GraniteClient(
        ILogger<GraniteClient> logger,
        GraniteServer server,
        ConnectionContext connection)
    {
        this.logger = logger;
        this.server = server;
        Connection = connection;

        state = GraniteClientState.Handshaking;
        keepAliveTimer = new Timer(10);
    }

    public async Task ConnectAsync()
    {
        var message = await Connection.ReadAsync();
        message.As<HandshakePacket>(out var handshakePacket);

        state = handshakePacket.NextState switch
        {
            1 => GraniteClientState.Status,
            2 => GraniteClientState.Login,
            _ => GraniteClientState.Disconnecting
        };

        logger.LogInformation("Switched to \"{State}\" state", state);

        var elapsedSeconds = 0D;

        try
        {
            while (true)
            {
                var old = DateTime.Now;

                try
                {
                    message = await Connection.ReadAsync();

                    switch (state)
                    {
                        case GraniteClientState.Status:
                            await HandleStatusStateAsync(message);
                            break;

                        case GraniteClientState.Login:
                            await HandleLoginStateAsync(message);
                            break;

                        case GraniteClientState.Play:
                            await HandlePlayStateAsync(message, elapsedSeconds);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                elapsedSeconds += DateTime.Now.Subtract(old).TotalSeconds;
            }
        }
        catch (ConnectionResetException)
        {
            logger.LogWarning("Connection reset");
        }

        if (state is not GraniteClientState.Disconnecting)
        {
            await DisconnectAsync();
        }
    }

    private async Task HandleStatusStateAsync(Message message)
    {
        switch (message.Type)
        {
            case 0x00:
                message.As<StatusRequestPacket>(out _);

                await Connection.WriteAsync(new StatusResponsePacket()
                {
                    Status = server.Status
                });
                break;

            case 0x01:
                message.As<PingRequestPacket>(out var pingRequestPacket);

                await Connection.WriteAsync(new PongResponsePacket()
                {
                    Payload = pingRequestPacket.Payload
                });

                await DisconnectAsync();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task HandleLoginStateAsync(Message message)
    {
        message.As<LoginStartPacket>(out var loginStartPacket);
        var guid = Guid.NewGuid();

        await Connection.WriteAsync(new LoginSuccessPacket()
        {
            Guid = guid.ToString(),
            Username = loginStartPacket.Username
        });

        state = GraniteClientState.Play;
        logger.LogInformation("Switched to \"{State}\" state", state);

        if (server.Session.HasStarted)
        {
            await Connection.WriteAsync(new DisconnectPacket()
            {
                Reason = Chat.Create("Come back later!")
            });

            await DisconnectAsync();
            return;
        }

        // Those packets are part of the play state, but moving them to the play state
        // method will cause it to get sent every single time, which is not what we want.
        await HandleInitialPlayStateSequenceAsync(loginStartPacket.Username, guid);
    }

    private async Task HandleInitialPlayStateSequenceAsync(string username, Guid guid)
    {
        var id = Random.Shared.Next();

        await Connection.WriteAsync(new JoinGamePacket()
        {
            Id = id
        });

        await Connection.WriteAsync(new PlayerPositionAndLookPacket()
        {
            X = 23.5,
            Y = 3,
            Z = 25.5,
            Yaw = -45,
            Pitch = 0
        });

        Player = new MinecraftPlayer(this, username, guid, id)
        {
            X = 23.5,
            Y = 3,
            Z = 25.5,
            Yaw = -45,
            Pitch = 0
        };
        await server.AddClientAsync(this);

        // await server.UpdatePlayerListItemAsync();
        // await server.SpawnPlayerAsync(this);

        await server.SendChatMessageAsync(Chat.Create(
            $"{Player.Username} joined the server",
            color: "yellow"));

        var region = await server.Lobby.LoadAsync();

        foreach (var chunk in region.LoadedChunks)
        {
            await Connection.WriteAsync(new ChunkPacket()
            {
                Chunk = chunk,
                X = chunk.Position.X,
                Z = chunk.Position.Z
            });
        }

        await server.BuildRandomFloorAsync();
    }

    private async Task HandlePlayStateAsync(Message message, double elapsedTime)
    {
        await Connection.WriteAsync(new TimeUpdatePacket()
        {
            Time = 6000
        });

        if (keepAliveTimer.CanTick(elapsedTime))
        {
            logger.LogDebug("Sent keep alive packet");
            await Connection.WriteAsync(new KeepAlivePacket());
        }

        switch (message.Type)
        {
            case 0x01:
                message.As<ChatMessagePacket>(out var chatMessagePacket);

                await server.SendChatMessageAsync(
                    Chat.Create($"{Player!.Username}: {chatMessagePacket.Message.Text}"),
                    chatMessagePacket.Position);

                break;

            case 0x04:
                message.As<PlayerPositionPacket>(out var playerPositionPacket);

                await server.UpdatePlayerPositionAsync(
                    this,
                    (sbyte) ((playerPositionPacket.X - Player!.X) * 32.0D),
                    (sbyte) ((playerPositionPacket.Y - Player.Y) * 32.0D),
                    (sbyte) ((playerPositionPacket.Z - Player.Z) * 32.0D),
                    playerPositionPacket.OnGround);

                Player.X = playerPositionPacket.X;
                Player.Y = playerPositionPacket.Y;
                Player.Z = playerPositionPacket.Z;
                Player.OnGround = playerPositionPacket.OnGround;

                if (playerPositionPacket.Y < -10)
                {
                    await Player!.KickAsync(Chat.Create("You lost!", color: "light_purple"));
                }

                break;

            case 0x05:
                message.As<PlayerLookPacket>(out var playerLookPacket);

                Player!.Yaw = playerLookPacket.Yaw;
                Player.Pitch = playerLookPacket.Pitch;
                Player.OnGround = playerLookPacket.OnGround;

                await server.UpdatePlayerLookAsync(Player!);
                break;

            default:
                logger.LogTrace("Ignoring packet type {Type}", message.Type);
                break;
        }
    }

    public async Task DisconnectAsync(string reason = "No specified reason")
    {
        Debug.Assert(state is not GraniteClientState.Disconnecting);

        state = GraniteClientState.Disconnecting;
        await server.RemoveClientAsync(this);
        
        if (Player is not null)
        {
            await server.DestroyEntityAsync(Player.Id);
            
            await server.UpdatePlayerListItemAsync(new RemovePlayerAction()
            {
                Player = Player
            });

            await server.SendChatMessageAsync(Chat.Create(
                $"{Player.Username} left the server",
                color: "yellow"));
        }

        Connection.Abort(new ConnectionAbortedException(reason));
        await Connection.DisposeAsync();

        logger.LogDebug("Aborted connection");
    }
}