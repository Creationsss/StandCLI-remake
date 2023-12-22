using System.Diagnostics;
using System.Text.RegularExpressions;

namespace StandCLI.Handlers
{
    public partial class MenuOptionsHandler
    {
        private static int selectedOption = 0;
        public static string CurrentMenu = string.Empty;

        public static void MenuOptions(string Option)
        {
            CurrentMenu = Option;
            Console.CursorVisible = false;
            ConsoleKey key;

            string[] MenuPicked = (string[])Program.ReturnMenus()[Option];

            for (int i = 0; i < MenuPicked.Length; i++)
            {
                Console.WriteLine(MenuPicked[i]);
            }

            do
            {
                Console.SetCursorPosition(0, 0);

                for (int i = 0; i < MenuPicked.Length; i++)
                {
                    if (i == selectedOption)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    Console.WriteLine(MenuPicked[i]);
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selectedOption = (selectedOption - 1 + MenuPicked.Length) % MenuPicked.Length;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedOption = (selectedOption + 1) % MenuPicked.Length;
                }
                else if (key == ConsoleKey.Enter)
                {
                    if (Option == "MainMenu")
                    {
                        HandleMainMenuOptions(selectedOption);
                    }
                    else if (Option == "StandFile")
                    {
                        HandleStandFileMenuOptions(selectedOption);
                    }
                    else if (Option == "StandDLL")
                    {
                        HandleStandDLLOptions(selectedOption);
                    }
                    else if (Option == "LauncherOptions")
                    {
                        HandleLauncherOptions(selectedOption);
                    }
                }

            } while (key != ConsoleKey.Escape && key != ConsoleKey.Q && key != ConsoleKey.LeftArrow && key != ConsoleKey.RightArrow);

            if (key == ConsoleKey.LeftArrow)
            {
                string CurrentOption = MenuPicked[selectedOption];
                if(CurrentOption.StartsWith("Auto inject delay:"))
                {
                    string[] sv_split = CurrentOption.Split(":");
                    int delay = int.Parse(sv_split[1].Replace("ms", String.Empty).Trim());
                    delay -= 1000;
                    Program.IniFile?.SetValue("Settings", "autoInjectDelay", delay.ToString());
                    Program.ReloadFileOptions();
                    Console.Clear();
                    MenuOptions("StandFile");
                }
                else 
                {
                    Console.Clear();
                    MenuOptions(CurrentMenu);
                }
            }
            else if (key == ConsoleKey.RightArrow)
            {
                string CurrentOption = MenuPicked[selectedOption];
                if(CurrentOption.StartsWith("Auto inject delay:"))
                {
                    string[] sv_split = CurrentOption.Split(":");
                    int delay = int.Parse(sv_split[1].Replace("ms", String.Empty).Trim());
                    delay += 1000;
                    Program.IniFile?.SetValue("Settings", "autoInjectDelay", delay.ToString());
                    Program.ReloadFileOptions();
                    Console.Clear();
                    MenuOptions("StandFile");
                }
                else 
                {
                    Console.Clear();
                    MenuOptions(CurrentMenu);
                }
            }
            else if (key == ConsoleKey.Escape)
            {
                Console.Clear();
                selectedOption = 0;
                if(CurrentMenu == "MainMenu")
                {
                    Environment.Exit(0);
                }
                else
                {
                    MenuOptions("MainMenu");
                }
            }
            else if (key == ConsoleKey.Q)
            {
                Environment.Exit(0);
            }
            else
            {
                Console.Clear();
                MenuOptions(CurrentMenu);
            }

            Console.CursorVisible = true;
        }

        public static void HandleMainMenuOptions(int optionIndex)
        {
            Console.Clear();
            selectedOption = 0;

            switch (optionIndex)
            {
                case 0:
                    string inject = InjectMethods.Inject();
                    Console.Clear();
                    Console.WriteLine(inject);
                    Console.ReadLine();
                    Console.Clear();
                    MenuOptions("MainMenu");
                    break;
                case 1:
                    MenuOptions("StandDLL");
                    Console.ReadLine();
                    break;
                case 2:
                    MenuOptions("LauncherOptions");
                    Console.ReadLine();
                    break;
                case 3:
                    MenuOptions("StandFile");
                    Console.ReadLine();
                    break;
                case 4:
                    Environment.Exit(0);
                    break;
            }
        }

        private static bool ToggleIniSetting(string section, string key)
        {
            string currentValue = Program.IniFile?.ReadValue(section, key) ?? "true";
            bool isTrue = currentValue == "true";
            bool toggledValue = !isTrue;
            Program.IniFile?.SetValue(section, key, toggledValue ? "true" : "false");
            return toggledValue;
        }

        public static void HandleStandFileMenuOptions(int optionIndex)
        {
            Console.Clear();

            string[] sv_length = (string[])Program.ReturnMenus()["StandFile"];
            bool skipReadKey = false;

            if (optionIndex >= 0 && optionIndex < sv_length.Length)
            {
                string[] sv_split = sv_length[optionIndex].Split(" ");
                string CurrentOption = sv_length[optionIndex].Replace("\n", String.Empty);

                if (CurrentOption.Contains("Back"))
                {
                    selectedOption = 0;
                    MenuOptions("MainMenu");
                    return;
                }
                else if(CurrentOption.StartsWith("Auto inject:"))
                {
                    bool toggle = ToggleIniSetting("Settings", "autoInject");
                    if(toggle) Task.Run(() => AutoInjection.AutoInject());
                    Program.Settings["autoInject"] = toggle.ToString().ToLower();
                    skipReadKey = true;
                }
                else if (CurrentOption.StartsWith("Show disclaimer:"))
                {
                    ToggleIniSetting("Settings", "disclaimer");
                    skipReadKey = true;
                }
                else if(CurrentOption.StartsWith("Update check:"))
                {
                    ToggleIniSetting("Settings", "updateCheck");
                    skipReadKey = true;
                }
                else if (CurrentOption.StartsWith("Confirm options:"))
                {
                    ToggleIniSetting("Settings", "confirmOptions");
                    skipReadKey = true;
                }
                else if(CurrentOption.StartsWith("Auto inject delay:"))
                {
                    string[] local_split = CurrentOption.Split(":");
                    int delay = int.Parse(local_split[1].Replace("ms", String.Empty).Trim());

                    Console.Clear();
                    Console.WriteLine("Please enter the new delay in milliseconds:");

                    string? newDelay = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newDelay) || !int.TryParse(newDelay, out int newDelayInt))
                    {
                        Console.WriteLine("You did not enter a valid delay.");
                        Console.ReadKey();
                        return;
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine($"Set delay to {newDelayInt}ms");
                        Console.ReadKey();
                    }

                    Program.IniFile?.SetValue("Settings", "autoInjectDelay", newDelayInt.ToString());
                    Program.ReloadFileOptions();
                    skipReadKey = true;
                }
                else if(CurrentOption.StartsWith("GTA Path:"))
                {
                    Console.Clear();
                    Console.WriteLine("Please enter the Grand Theft Auto V path:");
                    string? gtaPath = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(gtaPath))
                    {
                        Console.WriteLine("You did not enter a valid path.");
                    }
                    else
                    {
                        if(Directory.Exists(gtaPath))
                        {
                            Program.IniFile?.SetValue("Settings", "gtaPath", gtaPath);
                            skipReadKey = true;
                        }
                        else 
                        {
                            Console.WriteLine("The path you entered does not exist.");
                        }
                    }
                }
                else if (CurrentOption.StartsWith("StandCLI Logs and Settings:"))
                {
                    Process.Start("explorer.exe", Program.StandCLIFolder);
                    skipReadKey = true;
                }
                else if(CurrentOption.StartsWith("Stand Folder:"))
                {
                    Process.Start("explorer.exe", Program.StandFolder);
                    skipReadKey = true;
                }
                else if(CurrentOption == "Delete Stand Temp Folder")
                {
                    if(!ConfirmOption("delete the stand temp folder")) return;
                    if (FolderExists.CheckFolderExists(Path.Combine(Program.StandBinFolder, "Temp"), false) != "null")
                    {
                        Directory.Delete(Path.Combine(Program.StandBinFolder, "Temp"), true);
                        Console.WriteLine("Deleted Stand Temp Folder");
                    }
                }
                else if(CurrentOption == "Delete Stand Folder")
                {
                    if(!ConfirmOption("delete the stand folder")) return;
                    if (FolderExists.CheckFolderExists(Program.StandFolder, false) != "null")
                    {
                        Directory.Delete(Program.StandFolder, true);
                        Console.WriteLine("Deleted Stand Folder");
                    }
                }
                else if(CurrentOption == "Delete All stand versions")
                {
                    if(!ConfirmOption("delete all stand versions")) return;
                    if (FolderExists.CheckFolderExists(Program.StandBinFolder, false) != "null")
                    {
                        foreach (string file in Directory.GetFiles(Program.StandBinFolder, "Stand_*.dll"))
                        {
                            try { File.Delete(file); }
                            catch { }
                        }
                        Console.WriteLine("Deleted All stand versions");
                    }
                }
                else if(CurrentOption.StartsWith("Delete Stand DLL"))
                {
                    string ToDelete = Regex.Replace(sv_split[3], @"\(([^)]*)\)", "$1").Trim();
                    if(!ConfirmOption($"delete Stand {ToDelete}")) return;
                    if (File.Exists(Path.Combine(Program.StandBinFolder, $"Stand_{ToDelete}.dll")))
                    {
                        File.Delete(Path.Combine(Program.StandBinFolder, $"Stand_{ToDelete}.dll"));
                        Console.WriteLine($"Deleted Version {ToDelete}");
                    }
                }
                if (!skipReadKey) Console.ReadKey();
                Program.ReloadAllOptions();

                Console.Clear();
                int length = ReturnLength("StandFile");
                if (length <= selectedOption)
                {
                    selectedOption = 0;
                }
                MenuOptions("StandFile");
            }
            else
            {
                Console.WriteLine("Invalid option index.");
                Console.ReadKey();
            }
        }

        public static bool ConfirmOption(string Option)
        {
            string ConfirmOptions = Program.IniFile?.ReadValue("Settings", "confirmOptions") ?? "true";
            if(ConfirmOptions == "false") return true;

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Are you sure you want to {Option}?");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press y to confirm or any other key to cancel\n");
            Console.ResetColor();
            Console.CursorVisible = true;
            ConsoleKey key = Console.ReadKey(true).Key;
            Console.Clear();
            Console.CursorVisible = false;
            return key.ToString().ToLower() == "y";
        }

        public static int ReturnLength(string Option)
        {
            Dictionary<string, object> menus = Program.ReturnMenus();
            if (menus.ContainsKey(Option) && menus[Option] is string[] standFileOptions)
            {
                int length = standFileOptions.Length;
                return length;
            }
            return 0;
        }

        public static void HandleStandDLLOptions(int optionIndex)
        {
            Console.Clear();
            string[] sv_length = (string[])Program.ReturnMenus()["StandDLL"];

            if (optionIndex >= 0 && optionIndex < sv_length.Length)
            {
                string[] sv_split = sv_length[optionIndex].Split(" ");
                if (sv_split[0].Contains("Back"))
                {
                    selectedOption = 0;
                    MenuOptions("MainMenu");
                    return;
                }
                string binFolder = FolderExists.CheckFolderExists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand", "Bin"), true);
                Program.StandBinFolder = binFolder;

                Task<bool> downloadTask = NetworkHandler.DownloadStandDll(sv_split[0], Path.Combine(binFolder, $"Stand_{sv_split[0]}.dll"));
                downloadTask.Wait();

                if (downloadTask.Result)
                {
                    Console.ReadKey();
                    Program.UsingStandVersion(sv_split[0], true);
                    Program.ReloadAllOptions();
                    Console.Clear();
                    MenuOptions("StandDLL");
                }
                else
                {
                    Console.WriteLine("Download failed. Press any key to continue.");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Invalid option index.");
                Console.ReadKey();
            }
        }

        public static void HandleLauncherOptions(int optionIndex)
        {
            Console.Clear();

            string[] sv_length = (string[])Program.ReturnMenus()["LauncherOptions"];

            if (optionIndex >= 0 && optionIndex < sv_length.Length)
            {
                string option = sv_length[optionIndex];
                if(option.Contains("Back"))
                {
                    selectedOption = 0;
                    MenuOptions("MainMenu");
                    return;
                }

                if (option.Equals("Create Launcher"))
                {
                    string? gtaPath = Program.IniFile?.ReadValue("Settings", "gtaPath");

                    Console.Clear();
                    if(gtaPath == null || !Directory.Exists(gtaPath))
                    {
                        string? gtaPathCheckSteam = new FindGtaFolder().GetGtaPath();
                        if(gtaPathCheckSteam != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Found GTA V path from Steam: " + gtaPathCheckSteam);
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press y to confirm or any other key to enter your own\n");
                            ConsoleKey key = Console.ReadKey(true).Key;
                            if(key.ToString().ToLower() == "y")
                            {
                                gtaPath = gtaPathCheckSteam;
                            }
                            else
                            {
                                Console.WriteLine("Please enter the Grand Theft Auto V path:");
                                gtaPath = Console.ReadLine();
                            }
                        }
                        else {
                            Console.WriteLine("Please enter the Grand Theft Auto V path:");
                            gtaPath = Console.ReadLine();
                        }
                    }

                    if (string.IsNullOrWhiteSpace(gtaPath))
                    {
                        Program.logfile?.Log("Entered path was invalid!");
                        Console.WriteLine("You did not enter a valid path.");
                    }
                    else
                    {
                        Program.logfile?.Log("Path: " + gtaPath + " Was correct! Continuing...");
                        Program.IniFile?.SetValue("Settings", "gtaPath", gtaPath);
                        string CreateLauncherReturn = LauncherCreation.CreateLauncher();

                        Console.Clear();
                        Console.WriteLine(CreateLauncherReturn);
                        Program.logfile?.Log(CreateLauncherReturn);
                        Console.ReadKey();
                    }
                }
                else if(option.StartsWith("Launcher Path:"))
                {
                    string launcherPath = Program.IniFile?.ReadValue("Settings", "launcherPath") ?? "null";
                    launcherPath = launcherPath.Replace("\\PlayGTAV.exe", String.Empty);
                    if (launcherPath == "null")
                    {
                        Console.WriteLine("Launcher path not found.");
                        Program.logfile?.Log("Launcher path has not been set!");
                        Console.ReadKey();
                    }
                    Process.Start("explorer.exe", launcherPath);
                }
                else if(option.Equals("Reinstall Launcher"))
                {
                    if(!ConfirmOption("reinstall the launcher")) return;
                    string ReinstallLauncher = LauncherCreation.ReinstallLauncher();

                    Console.Clear();
                    Console.WriteLine(ReinstallLauncher);
                    Program.logfile?.Log(ReinstallLauncher);
                    Console.ReadKey();
                }
                else if(option.Equals("Delete Launcher"))
                {
                    if(!ConfirmOption("delete the launcher")) return;
                    string DeleteLauncher = LauncherCreation.DeleteLauncher();
                    Program.IniFile?.DeleteValue("Settings", "launcherPath");

                    Console.Clear();
                    Console.WriteLine(DeleteLauncher);
                    Program.logfile?.Log(DeleteLauncher);
                    Console.ReadKey();
                }

                Program.ReloadAllOptions();
                Console.Clear();
                selectedOption = 0;
                MenuOptions("LauncherOptions");
            }
        }
    }
}
