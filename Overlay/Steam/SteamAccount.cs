using Microsoft.Win32;

namespace Overlay.Steam
{
    internal static class SteamAccount
    {
        private const long SteamId64Base = 76561197960265728;

        public static string GetUserId()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam\ActiveProcess"))
            {
                if (key?.GetValue("ActiveUser") is int accountId && accountId != 0)
                {
                    return $"{SteamId64Base + (uint)accountId}@steam";
                }
            }

            return null;
        }
    }
}
