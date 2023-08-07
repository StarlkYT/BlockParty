namespace Starlk.Console.Networking.Packets.Play;

internal sealed class EntityLookPacket : IOutgoingPacket
{
    public int Type => 0x16;

    public required int Id { get; init; }

    public required byte Yaw { get; init; }
    public required byte Pitch { get; init; }
    public required bool OnGround { get; init; }

    public int CalculateLength()
    {
        return VariableIntegerHelper.GetBytesCount(Id) + sizeof(byte) * 3;
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteVariableInteger(Id);
        writer.WriteByte(Yaw);
        writer.WriteByte(Pitch);
        writer.WriteBoolean(OnGround);
        return writer.Position;
    }
}