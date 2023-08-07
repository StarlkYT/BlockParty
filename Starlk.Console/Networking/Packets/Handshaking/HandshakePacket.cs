namespace Starlk.Console.Networking.Packets.Handshaking;

internal sealed class HandshakePacket : IIngoingPacket<HandshakePacket>
{
    public int Type => 0x00;

    public required int ProtocolVersion { get; init; }

    public required string Address { get; init; }

    public required ushort Port { get; init; }

    public required int NextState { get; init; }

    public static HandshakePacket Read(ReadOnlySpan<byte> payload)
    {
        var reader = new SpanReader(payload);

        return new HandshakePacket()
        {
            ProtocolVersion = reader.ReadVariableInteger(),
            Address = reader.ReadString(),
            Port = reader.ReadUnsignedShort(),
            NextState = reader.ReadVariableInteger()
        };
    }
}