using System.Diagnostics;

namespace StandCLI.handlers
{
    class LauncherCreation
    {
        public static void RunningAsLauncher()
        {
            string currentName = Process.GetCurrentProcess().ProcessName;
            if (currentName == "PlayGTAV")
            {
                if (File.Exists("_PlayGTAV.exe"))
                {
                    string[] args = Environment.GetCommandLineArgs();

                    args = args.Skip(1).ToArray();

                    string argsString = string.Join(" ", args);

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "_PlayGTAV.exe",
                        Arguments = argsString,
                        UseShellExecute = false
                    };

                    Process.Start(startInfo);
                }
            }
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
                        File.Copy(defaultGTAEXE, Path.Combine(gtaPath, "_PlayGTAV.exe"));
                        File.Delete(defaultGTAEXE);
                        
                        try
                        {
                            if (currentExeFullPath != null)
                            {
                                string destPath = Path.Combine(gtaPath, "PlayGTAV.exe");
                                File.Copy(currentExeFullPath, destPath);

                                Program.IniFile?.SetValue("Settings", "launcherPath", destPath);
                                Program.logfile?.Log("Succesfully installed StandCLI to GTA V.");
                                return "Successfully copied StandCLI to GTA V folder.";
                            }
                            return "Failed to copy StandCLI to GTA V folder.";
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
                        Program.IniFile?.SetValue("Settings", "launcherPath", Path.Combine(gtaPath, "_PlayGTAV.exe"));
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
                                File.Delete(defaultGTAEXE);
                                File.Copy(currentExeFullPath, Path.Combine(gtaPath, "PlayGTAV.exe"));
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
                                File.Delete(defaultGTAEXE);
                                File.Copy(Path.Combine(gtaPath, "_PlayGTAV.exe"), Path.Combine(gtaPath, "PlayGTAV.exe"));
                                File.Delete(Path.Combine(gtaPath, "_PlayGTAV.exe"));
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