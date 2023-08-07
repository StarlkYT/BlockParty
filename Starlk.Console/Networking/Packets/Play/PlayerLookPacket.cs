namespace Starlk.Console.Networking.Packets.Play;

internal sealed class PlayerLookPacket : IIngoingPacket<PlayerLookPacket>
{
    public int Type => 0x05;

    public required float Yaw { get; init; }
    public required float Pitch { get; init; }
    public required bool OnGround { get; init; }

    public static PlayerLookPacket Read(ReadOnlySpan<byte> payload)
    {
        var reader = new SpanReader(payload);

        return new PlayerLookPacket()
        {
            Yaw = reader.ReadFloat(),
            Pitch = reader.ReadFloat(),
            OnGround = reader.ReadBoolean()
        };
    }
}