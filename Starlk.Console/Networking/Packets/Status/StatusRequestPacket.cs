namespace Starlk.Console.Networking.Packets.Status;

internal sealed class StatusRequestPacket : IIngoingPacket<StatusRequestPacket>
{
    public int Type => 0x00;

    public static StatusRequestPacket Read(ReadOnlySpan<byte> payload)
    {
        return new StatusRequestPacket();
    }
}