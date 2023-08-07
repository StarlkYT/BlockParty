using Starlk.Console.Components;

namespace Starlk.Console.Networking.Packets.Status;

internal sealed class StatusResponsePacket : IOutgoingPacket
{
    public int Type => 0x00;

    public required ServerStatus Status { get; init; }

    private string? serializedStatus;

    public int CalculateLength()
    {
        serializedStatus = Status.Serialize();
        return VariableStringHelper.GetBytesCount(serializedStatus);
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteString(serializedStatus!);
        return writer.Position;
    }
}