namespace Starlk.Console.Networking.Packets.Status;

internal sealed class PingRequestPacket : IIngoingPacket<PingRequestPacket>
{
    public int Type => 0x01;

    public required long Payload { get; init; }

    public static PingRequestPacket Read(ReadOnlySpan<byte> payload)
    {
        var reader = new SpanReader(payload);

        return new PingRequestPacket()
        {
            Payload = reader.ReadLong()
        };
    }
}