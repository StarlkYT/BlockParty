namespace Starlk.Console.Networking.Packets;

internal sealed record Message(int Type, byte[] Payload)
{
    public void As<T>(out T packet) where T : IIngoingPacket<T>
    {
        packet = T.Read(Payload);
    }
}

internal interface IPacket
{
    public int Type { get; }
}

internal interface IOutgoingPacket : IPacket
{
    public int CalculateLength();

    public int Write(ref SpanWriter writer);
}

internal interface IIngoingPacket<out TSelf> : IPacket where TSelf : IIngoingPacket<TSelf>
{
    public static abstract TSelf Read(ReadOnlySpan<byte> payload);
}