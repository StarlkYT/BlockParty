namespace Starlk.Console.Networking.Packets.Login;

internal sealed class LoginStartPacket : IIngoingPacket<LoginStartPacket>
{
    public int Type => 0x00;

    public required string Username { get; init; }

    public static LoginStartPacket Read(ReadOnlySpan<byte> payload)
    {
        var reader = new SpanReader(payload);

        return new LoginStartPacket()
        {
            Username = reader.ReadString()
        };
    }
}