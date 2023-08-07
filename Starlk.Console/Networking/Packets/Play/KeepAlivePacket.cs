namespace Starlk.Console.Networking.Packets.Play;

internal sealed class KeepAlivePacket : IOutgoingPacket
{
    public int Type => 0x00;

    public int CalculateLength()
    {
        return VariableIntegerHelper.GetBytesCount(Type);
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteVariableInteger(Type);
        return writer.Position;
    }
}