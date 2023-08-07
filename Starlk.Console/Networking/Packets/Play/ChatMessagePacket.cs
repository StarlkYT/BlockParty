using Starlk.Console.Components;

namespace Starlk.Console.Networking.Packets.Play;

internal enum ChatPosition
{
    ChatBox,
    SystemMessage,
    AboveHotBar
}

internal sealed class ChatMessagePacket : IOutgoingPacket, IIngoingPacket<ChatMessagePacket>
{
    public int Type => 0x02;

    // private int ingoingType => 0x01;

    public required Chat Message { get; init; }

    public ChatPosition Position { get; init; } = ChatPosition.ChatBox;

    private string? serializedMessage;

    public int CalculateLength()
    {
        serializedMessage = Message.Serialize();
        return VariableStringHelper.GetBytesCount(serializedMessage) + sizeof(byte);
    }

    public int Write(ref SpanWriter writer)
    {
        writer.WriteString(serializedMessage!);
        writer.WriteByte((byte) Position);

        return writer.Position;
    }

    public static ChatMessagePacket Read(ReadOnlySpan<byte> payload)
    {
        var reader = new SpanReader(payload);

        return new ChatMessagePacket()
        {
            Message = Chat.Create(reader.ReadString())
        };
    }
}