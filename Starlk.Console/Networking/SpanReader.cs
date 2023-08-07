using System.Buffers.Binary;
using System.Text;

namespace Starlk.Console.Networking;

internal ref struct SpanReader
{
    private int position;

    private readonly ReadOnlySpan<byte> span;

    public SpanReader(ReadOnlySpan<byte> buffer)
    {
        span = buffer;
    }

    public int ReadInteger()
    {
        return BinaryPrimitives.ReadInt32BigEndian(span[position..(position += sizeof(int))]);
    }

    public int ReadVariableInteger()
    {
        var numbersRead = 0;
        var result = 0;

        byte read;

        do
        {
            read = span[position++];

            var value = read & 0b01111111;
            result |= value << (7 * numbersRead);

            numbersRead++;

            if (numbersRead > 5)
            {
                throw new InvalidOperationException("Variable integer is too big.");
            }
        } while ((read & 0b10000000) != 0);

        return result;
    }

    public double ReadDouble()
    {
        return BinaryPrimitives.ReadDoubleBigEndian(span[position..(position += sizeof(double))]);
    }

    public short ReadShort()
    {
        return BinaryPrimitives.ReadInt16BigEndian(span[position..(position += sizeof(short))]);
    }

    public ReadOnlySpan<byte> ReadBytes()
    {
        return span[position..];
    }

    public byte ReadByte()
    {
        return span[position++];
    }

    public sbyte ReadSignedByte()
    {
        return (sbyte) span[position++];
    }

    public bool ReadBoolean()
    {
        return ReadByte() is 0;
    }

    public float ReadFloat()
    {
        return BinaryPrimitives.ReadSingleBigEndian(span[position..(position += sizeof(float))]);
    }

    public string ReadString()
    {
        var length = ReadVariableInteger();
        var buffer = span[position..(position += length)];
        return Encoding.UTF8.GetString(buffer);
    }

    public ushort ReadUnsignedShort()
    {
        return BinaryPrimitives.ReadUInt16BigEndian(span[position..(position += sizeof(ushort))]);
    }

    public long ReadLong()
    {
        return BinaryPrimitives.ReadInt64BigEndian(span[position..(position += sizeof(long))]);
    }
}