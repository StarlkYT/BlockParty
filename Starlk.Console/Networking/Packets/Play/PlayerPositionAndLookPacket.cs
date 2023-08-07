namespace Starlk.Console.Networking.Packets.Play;

internal sealed class PlayerPositionAndLookPacket : IOutgoingPacket
{
    public int Type => 0x08;

    public double X { get; init; }

    public double Y { get; init; }

    public double Z { get; init; }

    public float Yaw { get; init; }

    public float Pitch { get; init; }

    public byte Flags { get; init; } = 0x00;

    public int CalculateLength()
    {
        return sizeof(double)
               + sizeof(double)
               + sizeof(double)
               + sizeof(float)
               + sizeof(float)
               + sizeof(byte);
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteDouble(X);
        writer.WriteDouble(Y);
        writer.WriteDouble(Z);
        writer.WriteFloat(Yaw);
        writer.WriteFloat(Pitch);
        writer.WriteByte(Flags);

        return writer.Position;
    }
}