using System.Diagnostics;
using System.Text.RegularExpressions;

namespace StandCLI.Handlers
{
    public partial class MenuOptionsHandler
    {
        private static int selectedOption = 0;
        public static string CurrentMenu = String.Empty;

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

        public static void HandleStandFileMenuOptions(int optionIndex)
        {
            Console.Clear();

            string[] sv_length = (string[])Program.ReturnMenus()["StandFile"];
            bool skipReadKey = false;

            if (optionIndex >= 0 && optionIndex < sv_length.Length)
            {
                string[] sv_split = sv_length[optionIndex].Split(" ");
                if (sv_length[optionIndex] == "Auto inject: disabled")
                {
                    Program.Settings["autoInject"] = "true";
                    Program.IniFile?.SetValue("Settings", "autoInject", "true");
                    Task.Run(() => AutoInjection.AutoInject());
                    skipReadKey = true;
                }
                else if (sv_length[optionIndex] == "Auto inject: enabled")
                {
                    Program.Settings["autoInject"] = "false";
                    Program.IniFile?.SetValue("Settings", "autoInject", "false");
                    skipReadKey = true;
                }
                else if (sv_length[optionIndex].Contains("Back"))
                {
                    selectedOption = 0;
                    MenuOptions("MainMenu");
                    return;
                }

                else if(sv_length[optionIndex].StartsWith("Auto inject delay:"))
                {
                    string[] local_split = sv_length[optionIndex].Split(":");
                    int delay = int.Parse(local_split[1].Replace("ms", String.Empty).Trim());

                    Console.Clear();
                    Console.WriteLine("Please enter the new delay in milliseconds:\n");

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
                else if(sv_length[optionIndex].StartsWith("GTA Path:"))
                {
                    Console.Clear();
                    Console.WriteLine("Please enter the Grand Theft Auto V path:\n");
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
                            Console.WriteLine("\nThe path you entered does not exist.");
                        }
                    }
                }
                else if (sv_length[optionIndex].StartsWith("StandCLI Logs and Settings:"))
                {
                    Process.Start("explorer.exe", Program.StandCLIFolder);
                    skipReadKey = true;
                }
                else if(sv_length[optionIndex].StartsWith("Stand Folder:"))
                {
                    Process.Start("explorer.exe", Program.StandFolder);
                    skipReadKey = true;
                }
                else if (sv_length[optionIndex] == "Show disclaimer: enabled\n")
                {
                    Program.IniFile?.SetValue("Settings", "disclaimer", "false");
                    skipReadKey = true;
                }
                else if (sv_length[optionIndex] == "Show disclaimer: disabled\n")
                {
                    Program.IniFile?.SetValue("Settings", "disclaimer", "true");
                    skipReadKey = true;
                }
                else if(sv_length[optionIndex] == "Delete Stand Temp Folder")
                {
                    if (FolderExists.CheckFolderExists(Path.Combine(Program.StandBinFolder, "Temp"), false) != "null")
                    {
                        Directory.Delete(Path.Combine(Program.StandBinFolder, "Temp"), true);
                        Console.WriteLine("Deleted Stand Temp Folder");
                    }
                }
                else if(sv_length[optionIndex] == "Delete Stand Folder")
                {
                    if (FolderExists.CheckFolderExists(Program.StandFolder, false) != "null")
                    {
                        Directory.Delete(Program.StandFolder, true);
                        Console.WriteLine("Deleted Stand Folder");
                    }
                }
                else if(sv_length[optionIndex] == "Delete All stand versions\n")
                {
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
                else if(sv_length[optionIndex].StartsWith("Delete Stand DLL"))
                {
                    string ToDelete = Regex.Replace(sv_split[3], @"\(([^)]*)\)", "$1").Trim();
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
                        Console.WriteLine("Please enter the Grand Theft Auto V path:\n");
                        gtaPath = Console.ReadLine();
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
                    string ReinstallLauncher = LauncherCreation.ReinstallLauncher();

                    Console.Clear();
                    Console.WriteLine(ReinstallLauncher);
                    Program.logfile?.Log(ReinstallLauncher);
                    Console.ReadKey();
                }
                else if(option.Equals("Delete Launcher"))
                {
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
