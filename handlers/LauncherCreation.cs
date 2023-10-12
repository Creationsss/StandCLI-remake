using System.Diagnostics;
using System.Text.RegularExpressions;

namespace StandCLI.Handlers
{
    class LauncherCreation
    {
        public static void RunningAsLauncher()
        {
            
            string currentName = Process.GetCurrentProcess().ProcessName;
            if (currentName == "PlayGTAV")
            {
                DLLImports.AllocConsole();
                Program.logfile?.Log("Allocated console.");
                if (File.Exists("_PlayGTAV.exe"))
                {
                    Program.logfile?.Log("Found launcher!");
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
            Program.logfile?.Log("Sensitive info has been hidden.");
            return argsString;
        }

        public static string CreateLauncher()
        {
            Program.logfile?.Log("Checking if launcher exists already...");
            if (!CheckIfLauncherExists())
            {
                string? gtaPath = Program.IniFile?.ReadValue("Settings", "gtaPath");
                string? currentExeFullPath = Process.GetCurrentProcess().MainModule?.FileName;
                Program.logfile?.Log("Checking if gta path is valid...");

                if (gtaPath != null)
                {
                    string defaultGTAEXE = Path.Combine(gtaPath, "PlayGTAV.exe");
                    if (File.Exists(defaultGTAEXE))
                    {
                        try
                        {
                            Program.logfile?.Log("Found: " + defaultGTAEXE);
                            File.Copy(defaultGTAEXE, Path.Combine(gtaPath, "_PlayGTAV.exe"));
                            File.Delete(defaultGTAEXE);
                            
                            try
                            {
                                if (currentExeFullPath != null)
                                {
                                    string destPath = Path.Combine(gtaPath, "PlayGTAV.exe");
                                    File.Copy(currentExeFullPath, destPath);
                                    Program.logfile?.Log("Launcher was created succesfully!");
                                    Program.IniFile?.SetValue("Settings", "launcherPath", destPath);
                                    return "Successfully copied StandCLI to GTA V folder.";
                                }
                                Program.logfile?.Log("Path to executable is Invalid! Could not create launcher.");
                                return "Failed to copy StandCLI to GTA V folder.";
                            }
                            catch (Exception ex)
                            {
                                Program.logfile?.Log("An exception occurred (Please report this on github!) Exception:" + ex.ToString() + "Message:" + ex.Message);
                                return "Failed to copy StandCLI to GTA V folder.";
                            }
                        }
                        catch (Exception ex)
                        {
                            Program.logfile?.Log("Failed to copy StandCLI to GTA V (please open an issue on github!) Exception:" + ex.ToString());
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
                    Program.logfile?.Log("GTAPATH is set. continuing...");
                    string defaultGTAEXE = Path.Combine(gtaPath, "PlayGTAV.exe");

                    if (launcherPath != null)
                    {
                        Program.logfile?.Log("launcher path is set!");
                        if (Directory.Exists(gtaPath))
                        {
                            Program.logfile?.Log("Checking for gta path...");
                            if (File.Exists(defaultGTAEXE) && currentExeFullPath != null)
                            {
                                Program.logfile?.Log("Exists! Reinstalling now...");
                                try
                                {
                                    File.Delete(defaultGTAEXE);
                                    File.Copy(currentExeFullPath, Path.Combine(gtaPath, "PlayGTAV.exe"));
                                    Program.logfile?.Log("Done!");
                                }
                                catch (Exception ex)
                                {
                                    Program.logfile?.Log("There was an error! (Please report this on github!) Exception: " + ex.ToString());
                                    return "Failed to reinstall StandCLI to GTA V folder.";
                                }
                            }
                        }
                        else
                        {
                            Program.logfile?.Log("GTA path doesnt exist! (or isnt valid)");
                            return "GTA V path not specified.";
                        }
                    }
                    else
                    {
                        Program.logfile?.Log("Launcher could not be found! (does the path not exist?)");
                        return "Launcher not found.";
                    }
                }
                else
                {
                    Program.logfile?.Log("GTA path is not set. please set it before doing this!");
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
                    Program.logfile?.Log("GTAPATH is set. continuing...");
                    string defaultGTAEXE = Path.Combine(gtaPath, "PlayGTAV.exe");

                    if (launcherPath != null)
                    {
                        Program.logfile?.Log("launcher path is set!");
                        if (Directory.Exists(gtaPath))
                        {
                            Program.logfile?.Log("Checking for gta path...");
                            if (File.Exists(defaultGTAEXE))
                            {
                                Program.logfile?.Log("Exists! Deleting now...");
                                try 
                                {
                                    File.Delete(defaultGTAEXE);
                                    File.Copy(Path.Combine(gtaPath, "_PlayGTAV.exe"), Path.Combine(gtaPath, "PlayGTAV.exe"));
                                    File.Delete(Path.Combine(gtaPath, "_PlayGTAV.exe"));
                                    Program.logfile?.Log("Launcher deleted, PlayGTAV.exe is now street legal!");
                                }
                                catch (Exception ex)
                                {
                                    Program.logfile?.Log("Couldnt delete launcher! (do we not have permission? (Please report this!) ) Exception: " + ex.ToString());
                                    return "Failed to delete StandCLI from GTA V folder.";
                                }
                            }
                            else
                            {
                                Program.logfile?.Log("Could not find the original launcher!");
                                return "Launcher not found.";
                            }
                        }
                        else
                        {
                            Program.logfile?.Log("GTA path doesnt exist! (or isnt valid)");
                            return "GTA V path not specified.";
                        }
                    }
                    else
                    {
                        Program.logfile?.Log("Launcher could not be found! (does the path not exist?)");
                        return "Launcher not found.";
                    }
                }
                else
                {
                    Program.logfile?.Log("GTA path is not set. please set it before doing this!");
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