namespace Starlk.Console.Networking.Packets.Play;

internal sealed class SpawnPlayerPacket : IOutgoingPacket
{
    public int Type => 0x0C;

    public required int Id { get; init; }

    public required Guid Guid { get; init; }

    public double X { get; init; }

    public double Y { get; init; }

    public double Z { get; init; }

    public float Yaw { get; init; }

    public float Pitch { get; init; }

    public short CurrentItem { get; init; }

    public int CalculateLength()
    {
        var length = VariableIntegerHelper.GetBytesCount(Id)
                     + sizeof(long) * 2
                     + sizeof(int)
                     + sizeof(int)
                     + sizeof(int)
                     + sizeof(byte)
                     + sizeof(byte)
                     + sizeof(short);

        // Metadata end.
        length += sizeof(byte);

        return length;
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteVariableInteger(Id);
        writer.WriteGuid(Guid);
        writer.WriteFixedPointNumber(X);
        writer.WriteFixedPointNumber(Y);
        writer.WriteFixedPointNumber(Z);
        writer.WriteSignedByte(0);
        writer.WriteSignedByte(0);
        writer.WriteShort(CurrentItem);

        writer.WriteByte(0x7F);

        return writer.Position;
    }
}