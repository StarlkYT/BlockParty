using Starlk.Console.Components.Blocks;

namespace Starlk.Console.World;

internal sealed class Chunk
{
    public (int X, int Z) Position { get; }

    private readonly Section?[] sections;

    public Chunk(int x, int z)
    {
        Position = (x, z);
        sections = new Section[16];
    }

    public Section GetSection(Position position)
    {
        var index = position.Y >> 4;
        return sections[index] ?? (sections[index] = new Section());
    }

    public Block GetBlock(Position position)
    {
        var section = GetSection(position);
        return section.GetBlock(position % 16);
    }

    public void SetBlock(Block block, Position position)
    {
        var section = GetSection(position);
        section.SetBlock(block, position % 16);
    }

    public byte GetSkyLight(Position position)
    {
        var section = GetSection(position);
        return section.GetSkyLight(position % 16);
    }

    public void SetSkyLight(byte value, Position position)
    {
        var section = GetSection(position);
        section.SetSkyLight(value, position % 16);
    }

    public (byte[] Payload, ushort Bitmask) Serialize()
    {
        var serializedSections = sections
            .Where(section => section is not null)
            .Select(section => section!.Serialize())
            .ToArray();

        var payload = new List<byte>( /*12288 * serializedSections.Length + 256 */);

        // Each of blocks, blocks light and sky light must be added separately.
        foreach (var section in serializedSections)
        {
            payload.AddRange(section.Blocks);
        }

        foreach (var section in serializedSections)
        {
            payload.AddRange(section.BlocksLight);
        }

        foreach (var section in serializedSections)
        {
            payload.AddRange(section.SkyLight);
        }

        // Biomes.
        payload.AddRange(new byte[256]);

        return (payload.ToArray(), (ushort) ((1 << serializedSections.Length) - 1));
    }
}