using System.Diagnostics;
using System.Text.RegularExpressions;

namespace StandCLI.handlers
{
    class LauncherCreation
    {
        public static void RunningAsLauncher()
        {
            string currentName = Process.GetCurrentProcess().ProcessName;
            if (currentName == "PlayGTAV")
            {
                DLLImports.AllocConsole();
                if (File.Exists("_PlayGTAV.exe"))
                {
                    string[] args = Environment.GetCommandLineArgs();
                    args = args.Skip(1).ToArray();
                    string argsString = string.Join(" ", args);
                    
                    string maskedArgsString = MaskSensitiveInfo(argsString, new[] {"AUTH_PASSWORD", "epicusername", "epicuserid"});
                    
                    Program.logfile?.Log($"Running as launcher with args: {maskedArgsString}");

                    ProcessStartInfo startInfo = new()
                    {
                        FileName = "_PlayGTAV.exe",
                        Arguments = argsString,
                    };
                    Process.Start(startInfo);
                }
            }
        }

        private static string MaskSensitiveInfo(string argsString, string[] sensitiveKeys)
        {
            foreach (var key in sensitiveKeys)
            {
                var pattern = $@"({key}=\S+)";
                var replacement = $"{key}=[REDACTED]";
                argsString = Regex.Replace(argsString, pattern, replacement);
            }
            return argsString;
        }

        public static string CreateLauncher()
        {
            if (!CheckIfLauncherExists())
            {
                string? gtaPath = Program.IniFile?.ReadValue("Settings", "gtaPath");
                string? currentExeFullPath = Process.GetCurrentProcess().MainModule?.FileName;

                if (gtaPath != null)
                {
                    string defaultGTAEXE = Path.Combine(gtaPath, "PlayGTAV.exe");
                    if (File.Exists(defaultGTAEXE))
                    {
                        try
                        {
                            File.Copy(defaultGTAEXE, Path.Combine(gtaPath, "_PlayGTAV.exe"));
                            File.Delete(defaultGTAEXE);
                            
                            try
                            {
                                if (currentExeFullPath != null)
                                {
                                    string destPath = Path.Combine(gtaPath, "PlayGTAV.exe");
                                    File.Copy(currentExeFullPath, destPath);

                                    Program.IniFile?.SetValue("Settings", "launcherPath", destPath);
                                    return "Successfully copied StandCLI to GTA V folder.";
                                }
                                return "Failed to copy StandCLI to GTA V folder.";
                            }
                            catch (Exception)
                            {
                                return "Failed to copy StandCLI to GTA V folder.";
                            }
                        }
                        catch (Exception)
                        {
                            return "Failed to copy StandCLI to GTA V folder.";
                        }
                    }
                    else
                    {
                        return "GTA V not found in the specified path.";
                    }
                }
                else
                {
                    return "GTA V path not specified.";
                }
            }
            else
            {
                ReinstallLauncher();
                return "Launcher already exists.";
            }
        }
        
        public static bool CheckIfLauncherExists()
        {
            string? gtaPath = Program.IniFile?.ReadValue("Settings", "gtaPath");
            string? launcherPath = Program.IniFile?.ReadValue("Settings", "launcherPath");

            if (gtaPath != null)
            {
                if (Directory.Exists(gtaPath))
                {
                    bool launcherExists = File.Exists(Path.Combine(gtaPath, "_PlayGTAV.exe"));
                    if(launcherPath == null || launcherPath != Path.Combine(gtaPath, "_PlayGTAV.exe") && launcherExists)
                    {
                        Program.IniFile?.SetValue("Settings", "launcherPath", Path.Combine(gtaPath, "PlayGTAV.exe"));
                    }
                    
                    return launcherExists;
                }
                else
                {
                    Program.IniFile?.DeleteValue("Settings", "gtaPath");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static string ReinstallLauncher()
        {
            if(CheckIfLauncherExists())
            {
                string? gtaPath = Program.IniFile?.ReadValue("Settings", "gtaPath");
                string? launcherPath = Program.IniFile?.ReadValue("Settings", "launcherPath");
                string? currentExeFullPath = Process.GetCurrentProcess().MainModule?.FileName;

                if(gtaPath != null)
                {
                    string defaultGTAEXE = Path.Combine(gtaPath, "PlayGTAV.exe");

                    if (launcherPath != null)
                    {
                        if (Directory.Exists(gtaPath))
                        {
                            if (File.Exists(defaultGTAEXE) && currentExeFullPath != null)
                            {
                                try
                                {
                                    File.Delete(defaultGTAEXE);
                                    File.Copy(currentExeFullPath, Path.Combine(gtaPath, "PlayGTAV.exe"));
                                }
                                catch (Exception)
                                {
                                    return "Failed to reinstall StandCLI to GTA V folder.";
                                }
                            }
                        }
                        else
                        {
                            return "GTA V path not specified.";
                        }
                    }
                    else
                    {
                        return "Launcher not found.";
                    }
                }
                else
                {
                    return "GTA V path not specified.";
                }
                return "Successfully reinstalled StandCLI to GTA V folder.";
            }
            else
            {
                return "Launcher not found.";
            }

        }

        public static string DeleteLauncher()
        {
            if(CheckIfLauncherExists())
            {
                string? gtaPath = Program.IniFile?.ReadValue("Settings", "gtaPath");
                string? launcherPath = Program.IniFile?.ReadValue("Settings", "launcherPath");

                if(gtaPath != null)
                {
                    string defaultGTAEXE = Path.Combine(gtaPath, "PlayGTAV.exe");

                    if (launcherPath != null)
                    {
                        if (Directory.Exists(gtaPath))
                        {
                            if (File.Exists(defaultGTAEXE))
                            {
                                try 
                                {
                                    File.Delete(defaultGTAEXE);
                                    File.Copy(Path.Combine(gtaPath, "_PlayGTAV.exe"), Path.Combine(gtaPath, "PlayGTAV.exe"));
                                    File.Delete(Path.Combine(gtaPath, "_PlayGTAV.exe"));
                                }
                                catch (Exception)
                                {
                                    return "Failed to delete StandCLI from GTA V folder.";
                                }
                            }
                            else
                            {
                                return "Launcher not found.";
                            }
                        }
                        else
                        {
                            return "GTA V path not specified.";
                        }
                    }
                    else
                    {
                        return "Launcher not found.";
                    }
                }
                else
                {
                    return "GTA V path not specified.";
                }
                return "Successfully deleted StandCLI from GTA V folder.";
            }
            else
            {
                return "Launcher not found.";
            }
        }
    }
}