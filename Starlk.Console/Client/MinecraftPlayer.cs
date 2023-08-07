using Starlk.Console.Components;
using Starlk.Console.Networking;
using Starlk.Console.Networking.Packets.Play;

namespace Starlk.Console.Client;

internal sealed class MinecraftPlayer
{
    public string Username { get; }

    public Guid Guid { get; }

    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public float Yaw { get; set; }

    public float Pitch { get; set; }

    public bool OnGround { get; set; }
    
    public int Id { get; }

    private readonly GraniteClient client;

    public MinecraftPlayer(GraniteClient client, string username, Guid guid, int id)
    {
        this.client = client;
        Username = username;
        Guid = guid;
        Id = id;
    }

    public async Task PlaySoundEffectAsync(string name)
    {
        await client.Connection.WriteAsync(new SoundEffectPacket()
        {
            Effect = name,
            X = (int) X,
            Y = (int) Y,
            Z = (int) Z
        });
    }

    public async Task SendChatMessageAsync(Chat message, ChatPosition position = ChatPosition.AboveHotBar)
    {
        await client.Connection.WriteAsync(new ChatMessagePacket()
        {
            Message = message,
            Position = position
        });
    }

    public async Task TeleportPositionAsync(int x, int y, int z, int yaw = 0, int pitch = 0)
    {
        await client.Connection.WriteAsync(new PlayerPositionAndLookPacket()
        {
            X = x,
            Y = y,
            Z = z,
            Yaw = yaw,
            Pitch = pitch
        });
    }

    public async Task UpdatePlayerListItemAsync(PlayerListItemPacket packet)
    {
        await client.Connection.WriteAsync(packet);
    }

    public async Task KickAsync(Chat reason)
    {
        await client.Connection.WriteAsync(new DisconnectPacket()
        {
            Reason = reason
        });

        await client.DisconnectAsync(reason.Text);
    }
}