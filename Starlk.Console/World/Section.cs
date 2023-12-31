﻿using Starlk.Console.Components.Blocks;

namespace Starlk.Console.World;

internal sealed class Section
{
    private readonly byte[] blocks;
    private readonly byte[] blocksLight;
    private readonly byte[] skyLight;

    public Section()
    {
        blocks = new byte[8192];
        blocksLight = new byte[2048];
        skyLight = new byte[2048];
    }

    public Block GetBlock(Position position)
    {
        var index = AsIndex(position) * 2;

        var type = (blocks[index] >> 4) | (blocks[index + 1] << 4);
        var metadata = blocks[index] & 0x0F;

        return new Block()
        {
            Type = type,
            Metadata = metadata
        };
    }

    public void SetBlock(Block block, Position position)
    {
        var index = AsIndex(position) * 2;
        var type = block.Type;

        blocks[index] = (byte) ((type << 4) | block.Metadata);
        blocks[index + 1] = (byte) (type >> 4);
    }

    public byte GetSkyLight(Position position)
    {
        var index = AsIndex(position);
        return skyLight[index];
    }

    public void SetSkyLight(byte value, Position position)
    {
        var index = AsIndex(position);
        skyLight[index] = value;
    }

    public (byte[] Blocks, byte[] BlocksLight, byte[] SkyLight) Serialize()
    {
        for (var index = 0; index < 2048; index++)
        {
            // blocksLight[index] = 0xFF;
            skyLight[index] = 0xFF;
        }

        return (blocks, blocksLight, skyLight);
    }

    private static int AsIndex(Position position)
    {
        return position.Y << 8 | position.Z << 4 | position.X;
    }
}