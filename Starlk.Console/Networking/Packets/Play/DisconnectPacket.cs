using Starlk.Console.Components;

namespace Starlk.Console.Networking.Packets.Play;

internal sealed class DisconnectPacket : IOutgoingPacket
{
    public int Type => 0x40;

    public required Chat Reason { get; init; }

    private string? serializedReason;

    public int CalculateLength()
    {
        serializedReason = Reason.Serialize();
        return VariableStringHelper.GetBytesCount(serializedReason);
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteString(serializedReason!);
        return writer.Position;
    }
}