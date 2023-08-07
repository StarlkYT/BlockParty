using Starlk.Console.Components.Blocks;

namespace Starlk.Console.World;

internal sealed class Region
{
    public List<Chunk> LoadedChunks { get; } = new();

    public Chunk GetChunk(Position position, bool shift = true)
    {
        var (x, z) = shift ? (position.X >> 4, position.Z >> 4) : (position.X, position.Z);
        var chunk = LoadedChunks.FirstOrDefault(chunk => chunk.Position == (x, z));

        if (chunk is null)
        {
            chunk = new Chunk(x, z);
            LoadedChunks.Add(chunk);
        }

        return chunk;
    }

    public void SetChunk(Chunk chunk)
    {
        LoadedChunks.Add(chunk);
    }

    public Block GetBlock(Position position)
    {
        var chunk = GetChunk(position);
        return chunk.GetBlock(position);
    }

    public void SetBlock(Block block, Position position)
    {
        var chunk = GetChunk(position);
        chunk.SetBlock(block, position);
    }

    public byte GetSkyLight(Position position)
    {
        var chunk = GetChunk(position);
        return chunk.GetSkyLight(position);
    }

    public void SetSkyLight(byte value, Position position)
    {
        var chunk = GetChunk(position);
        chunk.SetSkyLight(value, position);
    }
}