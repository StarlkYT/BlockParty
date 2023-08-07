using Starlk.Console.World;

namespace Starlk.Console.Networking.Packets.Play;

internal sealed class ChunkPacket : IOutgoingPacket
{
    public int Type => 0x21;

    public Chunk? Chunk { get; init; }

    public int X { get; init; }

    public int Z { get; init; }

    private (byte[] payload, ushort bitmask)? serializedChunk;

    public int CalculateLength()
    {
        if (Chunk is null)
        {
            return sizeof(int)
                   + sizeof(int)
                   + sizeof(bool)
                   + sizeof(ushort)
                   + VariableIntegerHelper.GetBytesCount(Array.Empty<byte>().Length)
                   + Array.Empty<byte>().Length;
        }
        else
        {
            serializedChunk = Chunk.Serialize();

            return sizeof(int)
                   + sizeof(int)
                   + sizeof(bool)
                   + sizeof(ushort)
                   + VariableIntegerHelper.GetBytesCount(serializedChunk.Value.payload.Length)
                   + serializedChunk.Value.payload.Length;
        }
    }

    public int Write(ref SpanWriter writer)
    {
        if (Chunk is null)
        {
            writer.WriteInteger(X);
            writer.WriteInteger(Z);
            writer.WriteBoolean(true);
            writer.WriteUnsignedShort(0);
            writer.WriteBytes(Array.Empty<byte>());
        }
        else
        {
            writer.WriteInteger(X);
            writer.WriteInteger(Z);
            writer.WriteBoolean(true);
            writer.WriteUnsignedShort(serializedChunk!.Value.bitmask);
            writer.WriteVariableInteger(serializedChunk!.Value.payload.Length);
            writer.WriteBytes(serializedChunk!.Value.payload);
        }

        return writer.Position;
    }
}