namespace Starlk.Console.Networking.Packets.Play;

internal sealed class SetSlotPacket : IOutgoingPacket
{
    public int Type => 0x2F;

    public byte Window => 0;

    public short SlotIndex => 40;

    public required byte Block { get; init; }

    public required byte Metadata { get; init; }

    public int CalculateLength()
    {
        return sizeof(byte) + sizeof(short) + (Block == 0 ? 2 : 6);
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteByte(Window);
        writer.WriteShort(SlotIndex);

        if (Block == 0)
        {
            writer.WriteByte(0xFF);
            writer.WriteByte(0xFF);
        }
        else
        {
            writer.WriteByte(0);
            writer.WriteByte(Block);

            writer.WriteByte(1);
            writer.WriteByte(0);

            writer.WriteByte(Metadata);
            writer.WriteByte(0);
        }

        return writer.Position;
    }
}