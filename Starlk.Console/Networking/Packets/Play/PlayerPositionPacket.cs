namespace Starlk.Console.Networking.Packets.Play;

internal sealed class EntityTeleportPacket : IOutgoingPacket
{
    public int Type => 0x18;
    
    public required int Id { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Z { get; init; }
    public  byte Yaw { get; init; }
    public  byte Pitch { get; init; }
    public required bool OnGround { get; init; }
    
    public int CalculateLength()
    {
        return VariableIntegerHelper.GetBytesCount(Id) + sizeof(int) * 3 + sizeof(byte) * 3;
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteVariableInteger(Id);
        writer.WriteFixedPointNumber(X);
        writer.WriteFixedPointNumber(Y);
        writer.WriteFixedPointNumber(Z);
        writer.WriteByte(Yaw);
        writer.WriteByte(Pitch);
        writer.WriteBoolean(OnGround);
        return writer.Position;
    }
}

internal sealed class PlayerPositionPacket : IIngoingPacket<PlayerPositionPacket>
{
    public int Type => 0x06;

    public required double X { get; init; }

    public required double Y { get; init; }

    public required double Z { get; init; }
    
    public required bool OnGround { get; init; }

    public static PlayerPositionPacket Read(ReadOnlySpan<byte> payload)
    {
        var reader = new SpanReader(payload);

        return new PlayerPositionPacket
        {
            X = reader.ReadDouble(),
            Y = reader.ReadDouble(),
            Z = reader.ReadDouble(),
            OnGround = reader.ReadBoolean()
        };
    }
}