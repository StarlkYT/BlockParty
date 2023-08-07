using Starlk.Console.Components.Blocks;

namespace Starlk.Console.Networking.Packets.Play;

internal sealed class BlockChange
{
    public byte X { get; }
    public byte Y { get; }
    public byte Z { get; }
    public Block Block { get; set; }

    public BlockChange(byte x, byte y, byte z, Block block)
    {
        X = x;
        Y = y;
        Z = z;
        Block = block;
    }

    public int CalculateLength()
    {
        return sizeof(byte)
               + sizeof(byte)
               + VariableIntegerHelper.GetBytesCount(Block.Type << 4 | (Block.Metadata & 15));
    }

    public void Write(ref SpanWriter writer)
    {
        writer.WriteByte((byte) (X | (Z << 4)));
        writer.WriteByte(Y);
        writer.WriteVariableInteger(Block.Type << 4 | (Block.Metadata & 15));
    }
}

internal sealed class MultiBlockChangePacket : IOutgoingPacket
{
    public int Type => 0x22;

    public required int ChunkX { get; init; }

    public required int ChunkZ { get; init; }

    public required BlockChange[] Changes { get; init; }

    public int CalculateLength()
    {
        return sizeof(int)
               + sizeof(int)
               + VariableIntegerHelper.GetBytesCount(Changes.Length)
               + Changes.Sum(change => change.CalculateLength());
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteInteger(ChunkX);
        writer.WriteInteger(ChunkZ);
        writer.WriteVariableInteger(Changes.Length);

        foreach (var change in Changes)
        {
            change.Write(ref writer);
        }

        return writer.Position;
    }
}