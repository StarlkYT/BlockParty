namespace Starlk.Console.Networking.Packets.Play;

internal sealed class JoinGamePacket : IOutgoingPacket
{
    public int Type => 0x01;

    public required int Id { get; init; }

    public byte GameMode { get; init; } = 2;

    public sbyte Dimension { get; init; } = 0;

    public byte Difficulty { get; init; } = 0;

    public byte MaxPlayers { get; init; } = 10;

    public string LevelType { get; init; } = "flat";

    public bool ReducedDebugInformation { get; init; }

    public int CalculateLength()
    {
        return sizeof(int)
               + sizeof(byte)
               + sizeof(sbyte)
               + sizeof(byte)
               + sizeof(byte)
               + VariableStringHelper.GetBytesCount(LevelType)
               + sizeof(bool);
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteInteger(Id);
        writer.WriteByte(GameMode);
        writer.WriteSignedByte(Dimension);
        writer.WriteByte(Difficulty);
        writer.WriteByte(MaxPlayers);
        writer.WriteString(LevelType);
        writer.WriteBoolean(ReducedDebugInformation);

        return writer.Position;
    }
}