using System.Text.RegularExpressions;

namespace StandCLI.handlers
{
    public partial class MenuOptionsHandler
    {
        private static int selectedOption = 0;

        public static void MenuOptions(string Option)
        {
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
                }

            } while (key != ConsoleKey.Escape && key != ConsoleKey.Q);

            Console.CursorVisible = true;
        }

        public static void HandleMainMenuOptions(int optionIndex)
        {
            Console.Clear();

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
                    selectedOption = 0;
                    MenuOptions("StandDLL");
                    Console.ReadLine();
                    break;
                case 2:
                    selectedOption = 0;
                    MenuOptions("StandFile");
                    Console.ReadLine();
                    break;
                case 3:
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
                    skipReadKey = true;
                }
                else if (sv_length[optionIndex] == "Auto inject: enabled")
                {
                    Program.Settings["autoInject"] = "false";
                    Program.IniFile?.SetValue("Settings", "autoInject", "false");
                    skipReadKey = true;
                }
                else if (sv_split[0].Contains("Back"))
                {
                    selectedOption = 0;
                    MenuOptions("MainMenu");
                    return;
                }
                if(sv_split[2].Contains("Folder"))
                {
                    if (FolderExists.CheckFolderExists(Program.StandFolder, false) != "null")
                    {
                        Directory.Delete(Program.StandFolder, true);
                        Console.WriteLine("Deleted Stand Folder");
                    }
                }
                else if(sv_split[1].Contains("all"))
                {
                    if (FolderExists.CheckFolderExists(Program.StandBinFolder, false) != "null")
                    {
                        Directory.Delete(Program.StandBinFolder, true);
                        Console.WriteLine("Deleted All stand dlls");
                    }
                }
                else{
                    if(sv_split[0].Contains("Delete"))
                    {
                        String ToDelete = Regex.Replace(sv_split[3], @"\(([^)]*)\)", "$1").Trim();
                        if (File.Exists(Path.Combine(Program.StandBinFolder, $"Stand_{ToDelete}.dll")))
                        {
                            File.Delete(Path.Combine(Program.StandBinFolder, $"Stand_{ToDelete}.dll"));
                            Console.WriteLine($"Deleted Version {ToDelete}");
                        }
                    }
                }
                if (!skipReadKey) Console.ReadKey();
                Program.ReloadFileOptions();
                Program.ReloadStandDLLMenuOptions();

                Console.Clear();
                selectedOption = 0;
                MenuOptions("StandFile");
            }
            else
            {
                Console.WriteLine("Invalid option index.");
                Console.ReadKey();
            }
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
                    Program.ReloadFileOptions();
                    Program.ReloadStandDLLMenuOptions();
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
    }
}
