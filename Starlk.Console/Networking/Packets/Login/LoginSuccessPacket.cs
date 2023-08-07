namespace Starlk.Console.Networking.Packets.Login;

internal sealed class LoginSuccessPacket : IOutgoingPacket
{
    public int Type => 0x02;

    public required string Guid { get; init; }

    public required string Username { get; init; }

    public int CalculateLength()
    {
        return VariableStringHelper.GetBytesCount(Guid)
               + VariableStringHelper.GetBytesCount(Username);
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteString(Guid);
        writer.WriteString(Username);
        return writer.Position;
    }
}