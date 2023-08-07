using Starlk.Console.Client;

namespace Starlk.Console.Networking.Packets.Play;

internal interface IAction
{
    public int CalculateLength();

    public void Write(ref SpanWriter writer);
}

internal sealed class AddPlayerAction : IAction
{
    public required MinecraftPlayer[] Players { get; init; }

    private const int Action = 0;
    private const int PropertiesCount = 0;
    private const int GameMode = 0;
    private const int Ping = 0;
    private const bool HasDisplayName = false;

    public int CalculateLength()
    {
        var totalSize = VariableIntegerHelper.GetBytesCount(Action)
                        + VariableIntegerHelper.GetBytesCount(Players.Length);

        foreach (var player in Players)
        {
            totalSize += sizeof(long) * 2; // GUID
            totalSize += VariableStringHelper.GetBytesCount(player.Username); // Username
            totalSize += VariableIntegerHelper.GetBytesCount(PropertiesCount); // Properties count
            totalSize += VariableIntegerHelper.GetBytesCount(GameMode); // Game mode
            totalSize += VariableIntegerHelper.GetBytesCount(Ping); // Ping
            totalSize += sizeof(byte); // Has display name
        }

        return totalSize;
    }

    public void Write(ref SpanWriter writer)
    {
        writer.WriteVariableInteger(Action);
        writer.WriteVariableInteger(Players.Length);

        foreach (var player in Players)
        {
            writer.WriteGuid(player.Guid);
            writer.WriteString(player.Username);
            writer.WriteVariableInteger(PropertiesCount);
            writer.WriteVariableInteger(GameMode);
            writer.WriteVariableInteger(Ping);
            writer.WriteBoolean(HasDisplayName);
        }
    }
}

internal sealed class RemovePlayerAction : IAction
{
    public required MinecraftPlayer Player { get; init; }

    private const int Action = 4;
    private const int PlayersCount = 1;

    public int CalculateLength()
    {
        return VariableIntegerHelper.GetBytesCount(Action)
               + VariableIntegerHelper.GetBytesCount(PlayersCount)
               + sizeof(long) * 2 * PlayersCount;
    }

    public void Write(ref SpanWriter writer)
    {
        writer.WriteVariableInteger(Action);
        writer.WriteVariableInteger(PlayersCount);
        writer.WriteGuid(Player.Guid);
    }
}

internal sealed class PlayerListItemPacket : IOutgoingPacket
{
    public int Type => 0x38;

    public required IAction Action { get; init; }

    public int CalculateLength()
    {
        return Action.CalculateLength();
    }

    public int Write(ref SpanWriter writer)
    {
        Action.Write(ref writer);
        return writer.Position;
    }
}

// internal sealed class PlayerListItemPacket : IOutgoingPacket
// {
//     public int Type => 0x38;
//
//     public required MinecraftPlayer[] Players { get; init; }
//
//     private const int Action = 0;
//     private const int PropertiesCount = 0;
//     private const int GameMode = 0;
//     private const int Ping = 0;
//     private const bool HasDisplayName = false;
//
//     public int CalculateLength()
//     {
//         var totalSize = VariableIntegerHelper.GetBytesCount(Action)
//                         + VariableIntegerHelper.GetBytesCount(Players.Length);
//
//         foreach (var player in Players)
//         {
//             totalSize += sizeof(long) * 2; // GUID
//             totalSize += VariableStringHelper.GetBytesCount(player.Username); // Username
//             totalSize += VariableIntegerHelper.GetBytesCount(PropertiesCount); // Properties count
//             totalSize += VariableIntegerHelper.GetBytesCount(GameMode); // Game mode
//             totalSize += VariableIntegerHelper.GetBytesCount(Ping); // Ping
//             totalSize += sizeof(byte); // Has display name
//         }
//
//         return totalSize;
//     }
//
//     public int Write(ref SpanWriter writer)
//     {
//         writer.WriteVariableInteger(Action);
//         writer.WriteVariableInteger(Players.Length);
//
//         foreach (var player in Players)
//         {
//             writer.WriteGuid(player.Guid);
//             writer.WriteString(player.Username);
//             writer.WriteVariableInteger(PropertiesCount);
//             writer.WriteVariableInteger(GameMode);
//             writer.WriteVariableInteger(Ping);
//             writer.WriteBoolean(HasDisplayName);
//         }
//
//         return writer.Position;
//     }
// }