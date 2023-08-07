namespace Starlk.Console.Networking.Packets.Status;

internal sealed class PongResponsePacket : IOutgoingPacket
{
    public int Type => 0x01;

    public required long Payload { get; init; }

    public int CalculateLength()
    {
        return sizeof(long);
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteLong(Payload);
        return writer.Position;
    }
}