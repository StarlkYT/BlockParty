namespace Starlk.Console.Networking.Packets.Play;

internal sealed class TimeUpdatePacket : IOutgoingPacket
{
    public int Type => 0x03;

    public long WorldAge { get; init; }

    public required long Time { get; init; }

    public int CalculateLength()
    {
        return sizeof(long) + sizeof(long);
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteLong(WorldAge);
        writer.WriteLong(Time);

        return writer.Position;
    }
}