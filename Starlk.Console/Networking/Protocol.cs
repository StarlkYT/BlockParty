using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Connections;
using Starlk.Console.Networking.Packets;

namespace Starlk.Console.Networking;

internal static class Protocol
{
    public static async Task<Message> ReadAsync(this ConnectionContext context)
    {
        var reader = context.Transport.Input;

        while (true)
        {
            var result = await reader.ReadAsync(context.ConnectionClosed);
            var buffer = result.Buffer;

            if (TryReadInternal(ref buffer, out var packet))
            {
                reader.AdvanceTo(buffer.Start);
                return packet;
            }

            reader.AdvanceTo(buffer.Start, buffer.End);
        }
    }

    public static async Task WriteAsync(this ConnectionContext context, IOutgoingPacket packet)
    {
        var writer = context.Transport.Output;
        WriteInternal(writer, packet);
        await writer.FlushAsync(context.ConnectionClosed);
    }

    private static bool TryReadInternal(ref ReadOnlySequence<byte> buffer, [NotNullWhen(true)] out Message? packet)
    {
        var reader = new SequenceReader<byte>(buffer);
        packet = null;

        if (!reader.TryReadVariableInteger(out var length)
            || !reader.TryReadVariableInteger(out var type))
        {
            return false;
        }

        if (!reader.TryReadExact(
                length - VariableIntegerHelper.GetBytesCount(type),
                out var payload))
        {
            return false;
        }

        packet = new Message(type, payload.ToArray());
        buffer = buffer.Slice(++length);

        return true;
    }

    private static void WriteInternal(PipeWriter writer, IOutgoingPacket packet)
    {
        var packetLength = packet.CalculateLength() + VariableIntegerHelper.GetBytesCount(packet.Type);
        var totalLength = packetLength + VariableIntegerHelper.GetBytesCount(packetLength);
        
        var spanWriter = new SpanWriter(writer.GetSpan(totalLength), totalLength);
        spanWriter.WriteVariableInteger(packetLength);
        spanWriter.WriteVariableInteger(packet.Type);
        
        var offset = packet.Write(ref spanWriter);
        writer.Advance(offset);
    }
}