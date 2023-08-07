namespace Starlk.Console.Networking.Packets.Play;

internal sealed class DestroyEntitiesPacket : IOutgoingPacket
{
    public int Type => 0x13;

    public required int[] Ids { get; init; }

    public int CalculateLength()
    {
        var length = VariableIntegerHelper.GetBytesCount(Ids.Length)
                     + Ids.Sum(VariableIntegerHelper.GetBytesCount);

        return length;
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteVariableInteger(Ids.Length);

        foreach (var id in Ids)
        {
            writer.WriteVariableInteger(id);
        }

        return writer.Position;
    }
}