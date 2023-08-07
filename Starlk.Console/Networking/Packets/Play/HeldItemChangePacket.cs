namespace Starlk.Console.Networking.Packets.Play;

internal sealed class HeldItemChangePacket : IOutgoingPacket
{
    public int Type => 0x09;

    public required byte Slot { get; init; }

    public int CalculateLength()
    {
        return sizeof(byte);
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteByte(Slot);
        return writer.Position;
    }
}