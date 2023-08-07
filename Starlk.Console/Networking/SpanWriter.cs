using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Starlk.Console.Networking;

internal ref struct SpanWriter
{
    public int Position { get; private set; }

    private readonly Span<byte> span;
    private readonly int bufferLength;

    public SpanWriter(Span<byte> span, int length)
    {
        this.span = span;
        bufferLength = length;
    }

    public void WriteByte(byte value)
    {
        EnsureCapacity();

        span[Position] = value;
        Position += sizeof(byte);
    }

    public void WriteSignedByte(sbyte value)
    {
        EnsureCapacity();

        span[Position] = (byte) value;
        Position += sizeof(sbyte);
    }

    public void WriteBoolean(bool value)
    {
        WriteByte((byte) (value ? 1 : 0));
    }

    public void WriteFixedPointNumber(double value)
    {
        EnsureCapacity();
        WriteInteger((int) (value * 32.0D));
    }

    public void WriteFixedPointNumberByte(sbyte value)
    {
        EnsureCapacity();
        WriteSignedByte((value));
    }

    public void WriteInteger(int value)
    {
        EnsureCapacity();

        BinaryPrimitives.WriteInt32BigEndian(span[Position..], value);
        Position += sizeof(int);
    }

    public void WriteLong(long value)
    {
        EnsureCapacity();

        BinaryPrimitives.WriteInt64BigEndian(span[Position..], value);
        Position += sizeof(long);
    }

    public void WriteDouble(double value)
    {
        EnsureCapacity();

        BinaryPrimitives.WriteDoubleBigEndian(span[Position..], value);
        Position += sizeof(double);
    }

    public void WriteFloat(float value)
    {
        EnsureCapacity();

        BinaryPrimitives.WriteSingleBigEndian(span[Position..], value);
        Position += sizeof(float);
    }

    public void WriteGuid(Guid value)
    {
        EnsureCapacity();

        if (value == Guid.Empty)
        {
            WriteLong(0L);
            WriteLong(0L);
        }
        else
        {
            var guid = BigInteger.Parse(
                value.ToString().Replace("-", ""),
                NumberStyles.HexNumber);

            WriteBytes(guid.ToByteArray(false, true));
        }
    }

    public void WriteString(string value)
    {
        EnsureCapacity();

        var length = Encoding.UTF8.GetByteCount(value);
        WriteVariableInteger(length);
        Position += Encoding.UTF8.GetBytes(value, span[Position..]);
    }

    public void WriteBytes(ReadOnlySpan<byte> value)
    {
        EnsureCapacity();

        value.CopyTo(span[Position..]);
        Position += value.Length;
    }

    public void WriteUnsignedShort(ushort value)
    {
        EnsureCapacity();

        BinaryPrimitives.WriteUInt16BigEndian(span[Position..], value);
        Position += sizeof(ushort);
    }

    public void WriteShort(short value)
    {
        EnsureCapacity();

        BinaryPrimitives.WriteInt16BigEndian(span[Position..], value);
        Position += sizeof(short);
    }

    public void WriteVariableInteger(int value)
    {
        EnsureCapacity();

        var unsigned = (uint) value;

        do
        {
            var current = (byte) (unsigned & 127);
            unsigned >>= 7;

            if (unsigned != 0)
            {
                current |= 128;
            }

            span[Position++] = current;
        } while (unsigned != 0);
    }

    private void EnsureCapacity()
    {
        if (Position >= bufferLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Position),
                Position,
                "Tried to write outside the packet's buffer");
        }
    }
}