namespace Starlk.Console.Client;

internal enum GraniteClientState
{
    Handshaking,
    Status,
    Login,
    Play,
    Disconnecting
}