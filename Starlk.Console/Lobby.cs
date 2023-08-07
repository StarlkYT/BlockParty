using Microsoft.Extensions.Logging;
using Raspite.Serializer;
using Raspite.Serializer.Tags;
using Starlk.Console.Client;
using Starlk.Console.Components.Blocks;
using Starlk.Console.Networking;
using Starlk.Console.Networking.Packets.Play;
using Starlk.Console.World;

namespace Starlk.Console;

internal sealed class Lobby
{
    public BlockChange? SampledBlock { get; private set; }

    private Region? loadedRegion;

    private int width;
    private int length;
    private int height;

    private BlockChange[] floorBlocks = new BlockChange[256];

    private readonly BlockChange[] airBlocks;
    private readonly ILogger<Lobby> logger;

    public Lobby(ILogger<Lobby> logger)
    {
        this.logger = logger;

        airBlocks = new BlockChange[256];
        var air = new Block();

        for (byte x = 0; x < 16; x++)
        {
            for (byte z = 0; z < 16; z++)
            {
                var index = x + z * 16;
                airBlocks[index] = new BlockChange(x, 0, z, air);
            }
        }
    }

    public async Task<Region> LoadAsync()
    {
        if (loadedRegion is not null)
        {
            return loadedRegion;
        }

        logger.LogDebug("Loading the lobby...");

        await using var stream = File.OpenRead("lobby.nbt");
        var tag = await BinaryTagSerializer.DeserializeAsync<CompoundTag>(stream);

        width = tag.First<ShortTag>("Width").Value;
        length = tag.First<ShortTag>("Length").Value;
        height = tag.First<ShortTag>("Height").Value;
        var blocks = tag.First<IntegerCollectionTag>("Blocks").Children;
        var metadata = tag.First<IntegerCollectionTag>("Metadata").Children;

        loadedRegion = new Region();

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                for (var z = 0; z < length; z++)
                {
                    var index = x + z * width + y * width * length;
                    loadedRegion.SetBlock(new Block(blocks[index], metadata[index]), new Position(x, y, z));
                }
            }
        }

        // Barrier blocks.
        loadedRegion.SetBlock(new Block(166), new Position(29, 2, 34));
        loadedRegion.SetBlock(new Block(166), new Position(29, 2, 33));
        loadedRegion.SetBlock(new Block(166), new Position(29, 2, 32));
        loadedRegion.SetBlock(new Block(166), new Position(32, 2, 31));
        loadedRegion.SetBlock(new Block(166), new Position(31, 2, 31));
        loadedRegion.SetBlock(new Block(166), new Position(30, 2, 31));

        // Temporary.
        loadedRegion.SetBlock(new Block(7), new Position(23, 0, 25));

        return loadedRegion;
    }

    public async Task UpdateFloorAsync(BlockChange[] floor, GraniteClient client)
    {
        floorBlocks = floor;
        SampledBlock = floor[Random.Shared.Next(floor.Length)];

        await FillAsync(floor, client);
    }

    public async Task DestroyFloorAsync(GraniteClient client)
    {
        await FillAsync(airBlocks, client);

        var newFloor = floorBlocks;

        foreach (var block in newFloor)
        {
            if (block.Block.Metadata != SampledBlock!.Block.Metadata)
            {
                block.Block = new Block();
            }
        }

        await FillAsync(newFloor, client);
    }

    private async Task FillAsync(BlockChange[] blocks, GraniteClient client)
    {
        var positions = new (byte X, byte Z)[]
        {
            (1, 1), (1, 2), (0, 1), (0, 2), (2, 1)
        };

        foreach (var position in positions)
        {
            await client.Connection.WriteAsync(new MultiBlockChangePacket()
            {
                ChunkX = position.X,
                ChunkZ = position.Z,
                Changes = blocks
            });
        }
    }
}