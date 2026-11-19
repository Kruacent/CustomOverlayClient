namespace Overlay.Network
{
    internal enum ConnectionStatus
    {
        Connecting,
        Connected,
        WaitingForGame,
        Unreachable,
        SteamNotRunning,
        Banned,
        AlreadyInUse
    }
}
