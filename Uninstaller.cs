using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ArchLight
{
    internal static class Uninstaller
    {
        internal static readonly string[] InstalledFiles = {
            "ArchLight.exe", "ArchLight.exe.bak", "ArchLight.exe.new",
            "ArcLight.exe", "Uninstall.exe"
        };

        internal static void RemoveFiles(string directory)
        {
            if (Directory.Exists(directory) && (File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("The installation directory is a link. Remove its files manually.");
            // Delete only installer-owned files; never recursively remove user data.
            foreach (string name in InstalledFiles) File.Delete(Path.Combine(directory, name));
            if (Directory.Exists(directory) && Directory.GetFileSystemEntries(directory).Length == 0)
                Directory.Delete(directory, false);
        }

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            try
            {
                string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ArcLight");
                foreach (string name in new[] { "ArchLight", "ArcLight" })
                    foreach (var process in Process.GetProcessesByName(name))
                        using (process)
                            if (!process.HasExited)
                            {
                                MessageBox.Show("Please choose Exit in ArchLight, then run uninstall again. This allows the app to restore your display and save your settings.", "ArchLight");
                                return;
                            }

                string installed = Path.Combine(directory, "Uninstall.exe");
                if (string.Equals(Path.GetFullPath(Application.ExecutablePath), Path.GetFullPath(installed), StringComparison.OrdinalIgnoreCase))
                {
                    string temporary = Path.Combine(Path.GetTempPath(), "ArchLight-Uninstall-" + Guid.NewGuid().ToString("N") + ".exe");
                    File.Copy(installed, temporary);
                    Process.Start(new ProcessStartInfo(temporary, "--cleanup " + Process.GetCurrentProcess().Id) { UseShellExecute = false, CreateNoWindow = true });
                    return;
                }
                int parentId;
                if (args.Length != 2 || args[0] != "--cleanup" || !int.TryParse(args[1], out parentId))
                    throw new InvalidOperationException("Start uninstall from Windows Installed apps.");
                try { using (var parent = Process.GetProcessById(parentId)) if (!parent.WaitForExit(10000)) throw new IOException("Uninstall is still running. Please try again."); }
                catch (ArgumentException) { }

                // A newly started app keeps its executable locked; stop on failure.
                RemoveFiles(directory);
                StartupRegistration.Set(false, "");
                foreach (var folder in new[] { Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolder.Programs })
                    foreach (string name in new[] { "ArchLight.lnk", "ArcLight.lnk" })
                        File.Delete(Path.Combine(Environment.GetFolderPath(folder), name));
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\ArcLight", false);
                MessageBox.Show("ArchLight has been removed. Your saved preferences have been kept.", "ArchLight");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ArchLight uninstall", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
