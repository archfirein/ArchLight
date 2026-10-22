using System;
using Microsoft.Win32;

namespace ArchLight
{
    internal static class StartupRegistration
    {
        internal static void Set(RegistryKey key, bool enabled, string executable)
        {
            if (key == null) throw new InvalidOperationException("Windows startup settings are unavailable.");
            if (enabled) key.SetValue("ArchLight", "\"" + executable + "\"");
            else key.DeleteValue("ArchLight", false);
            key.DeleteValue("ArcLight", false);
        }

        internal static void Set(bool enabled, string executable)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                Set(key, enabled, executable);
        }
    }
}
