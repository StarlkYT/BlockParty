namespace Starlk.Console.World;

internal sealed class NibbleArray
{
    private readonly byte[] buffer;

    public byte this[int index]
    {
        get => (byte) ((buffer[index / 2] >> (index % 2 * 4)) & 0xF);
        set
        {
            value &= 0xF;
            buffer[index / 2] &= (byte) (0xF << ((index + 1) % 2 * 4));
            buffer[index / 2] |= (byte) (value << (index % 2 * 4));
        }
    }

    public NibbleArray(int length)
    {
        buffer = new byte[length];
    }

    public static implicit operator byte[](NibbleArray array)
    {
        return array.buffer;
    }
}