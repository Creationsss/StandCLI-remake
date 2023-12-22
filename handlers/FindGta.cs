using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace StandCLI.Handlers
{
    public class FindGtaFolder
    {
        public string? GetGtaPath()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("This method is only supported on Windows.");
                return null;
            }

            try
            {
                const string steamRegPath = @"SOFTWARE\WOW6432Node\Valve\Steam";
                const string steamAppsPath = @"steamapps\common\Grand Theft Auto V";

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(steamRegPath))
                {
                    if (key != null)
                    {
                        object installPath = key.GetValue("InstallPath");
                        if (installPath != null)
                        {
                            var libraryFoldersPath = Path.Combine(installPath.ToString(), "steamapps", "libraryfolders.vdf");
                            var libraryPaths = GetLibraryPaths(libraryFoldersPath);

                            foreach (var libraryPath in libraryPaths)
                            {
                                string gtaPath = Path.Combine(libraryPath, steamAppsPath);
                                if (Directory.Exists(gtaPath))
                                {
                                    return gtaPath;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return null;
        }

        private List<string> GetLibraryPaths(string libraryFoldersPath)
        {
            var paths = new List<string>();

            try
            {
                var content = File.ReadAllText(libraryFoldersPath);
                var matches = Regex.Matches(content, "\"path\"\\s*\"(.+?)\"");

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        paths.Add(match.Groups[1].Value.Replace(@"\\", @"\"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading library folders: " + ex.Message);
            }

            return paths;
        }
    }
}
