namespace Starlk.Console.Networking.Packets.Play;

internal sealed class EntityRelativeMovePacket : IOutgoingPacket
{
    public int Type => 0x15;

    public required int Id { get; init; }

    public required sbyte X { get; init; }
    public required sbyte Y { get; init; }
    public required sbyte Z { get; init; }
    public required bool OnGround { get; init; }

    public int CalculateLength()
    {
        return VariableIntegerHelper.GetBytesCount(Id)
               + sizeof(byte)
               + sizeof(byte)
               + sizeof(byte)
               + sizeof(bool);
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteVariableInteger(Id);
        writer.WriteFixedPointNumberByte(X);
        writer.WriteFixedPointNumberByte(Y);
        writer.WriteFixedPointNumberByte(Z);
        writer.WriteBoolean(OnGround);
        return writer.Position;
    }
}