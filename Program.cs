using StandCLI.handlers;

namespace StandCLI
{
    class Program
    {
        private static readonly string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static IniFile? IniFile;

        public static Dictionary<string, string> Settings = new();
        private static readonly Dictionary<string, string> DefaultSettings = new()
        {
            {"disclaimer", "true"},
            {"autoInject", "false"},
            {"autoInjectDelay", "45000"}
        };

        private static string[] MainMenuOptions = {};
        private static string[] StandFileOptions = {};
        private static string[] StandDLLOptions = {};

        private static Dictionary<string, object> Menus = new();

        public static string StandFolder = FolderExists.CheckFolderExists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand"), false);
        public static string StandBinFolder = FolderExists.CheckFolderExists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand", "Bin"), false);

        public static string[] SupportedStandVersions = NetworkHandler.SupportedStandVersion().Result;

        public static string CurrentFullStandVersion = "";
        public static string CurrentStandDllVersion = "";

        public static string CurrentStandCLIVersion = "2.0";

        public static bool injected = false;

        public static Logger? logfile;

        static void Main(string[] args)
        {
            string StandCLIFolder = FolderExists.CheckFolderExists(Path.Combine(CurrentDirectory, "StandCLI"));
            IniFile = new(Path.Combine(StandCLIFolder, "settings.ini"));

            logfile = new(Path.Combine(StandCLIFolder, "logs.txt"));

            string[]? StandVersions = NetworkHandler.GetLatestStandVersion().Result;
            if (StandVersions == null)
            {
                Console.WriteLine("StandCLI", "Failed to get latest stand version, exiting...");
                Thread.Sleep(5000);
                Environment.Exit(0);
            }
            CurrentFullStandVersion = StandVersions[0];
            CurrentStandDllVersion = StandVersions[1];
            logfile = new("StandCLI.log");
            logfile.Log("StandCLI " + CurrentStandCLIVersion + " Reporting for duty!");

            CheckSettings();
            Disclaimer();
            SetMenuOptions();

            Task.Run(() => AutoInjection.AutoInject());

            MenuOptionsHandler.MenuOptions("MainMenu");
        }

        public static string? UsingStandVersion(string? version = null, bool set = false)
        {
            if (set)
            {
                if (version == null) return null;
                IniFile?.SetValue("Settings", "standVersion", version);
                return version;
            }
            else{
                string? stand_dll_ver = IniFile?.ReadValue("Settings", "standVersion");
                if(stand_dll_ver == null)
                {
                    return null;
                }
                else
                {
                    return stand_dll_ver;
                }
            }
        }

        static void CheckSettings()
        {
            foreach (string settingKey in DefaultSettings.Keys)
            {
                string? readSettingValue = IniFile?.ReadValue("Settings", settingKey);
                
                if (readSettingValue == null)
                {
                    IniFile?.SetValue("Settings", settingKey, DefaultSettings[settingKey]);
                    Settings.Add(settingKey, DefaultSettings[settingKey]);
                }
                else
                {
                    Settings.Add(settingKey, readSettingValue);
                }
            }
        }

        static void Disclaimer()
        {
            if(Settings["disclaimer"] == "false")
            {
                logfile?.Log("Disclaimer was skipped");
                goto skip;
            }

            Console.Clear();
            Console.Title = "Stand CLI Disclaimer";
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Disclaimer: ");
            Console.WriteLine("Im not sure if injecting stand this way is safe so use at your own risk");
            Console.WriteLine("If you have any issues with this fork make a issue on github or dm me on discord @ Creations");
            Console.WriteLine("StandCLI is NOT affiliated with Calamity, Inc. nor Rockstar Games or TakeTwo Interactive.");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\nUse the arrow keys to scroll and enter to select an option");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\nKnown problems:");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("> AutoInject will crash you if you are still on the main menu options screen when it tries to inject");
            Console.WriteLine("> Joining public sessions through RID seems to be unstable.");
            Console.WriteLine("> General Stability issues.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nPress c to never see this again or press any other key to continue");
            ConsoleKeyInfo choiceInfo = Console.ReadKey(intercept: true);
            char choiceChar = char.ToLower(choiceInfo.KeyChar);

            if (choiceChar == 'c')
            {
                IniFile?.SetValue("Settings", "disclaimer", "false");
                logfile?.Log("Disclaimer set to not be shown");
            }

            skip:
                Console.Clear();
                Console.Title = "Stand CLI version " + CurrentStandCLIVersion;
            
        }

        static void SetMenuOptions()
        {
            Console.Clear();

            MainMenuOptions = new string[]
            {
                "Inject Stand",
                "DLL Version Options",
                "Other Settings",
                "\nExit"
            };

            ReloadFileOptions();

            ReloadStandDLLMenuOptions();

            Menus.Add("MainMenu", MainMenuOptions);
        }

        public static Dictionary<string, object> ReturnMenus()
        {
            return Menus;
        }

        public static void ReloadStandDLLMenuOptions()
        {
            StandDLLOptions = new string[] { };
            foreach (string version in SupportedStandVersions)
            {
                string[] AddText = {};
                if(version == CurrentStandDllVersion) AddText = AddText.Append("latest").ToArray();


                if (File.Exists(Path.Combine(StandBinFolder, $"Stand_{version}.dll")))
                {
                    if (UsingStandVersion() == version) AddText = AddText.Append("using").ToArray();
                    AddText = AddText.Append("installed").ToArray();
                }

                string AddedText = "";
                foreach (string text in AddText)
                {
                    if (text == AddText[0] && AddText.Length == 1) AddedText += $"({text})";
                    else if (text == AddText[0]) AddedText += $"({text}, ";
                    else if (text == AddText[AddText.Length - 1]) AddedText += text + ")";
                    else if (text != AddText[0]) AddedText += text + ", ";
                }

                StandDLLOptions = StandDLLOptions.Append($"{version} {AddedText}").ToArray();
            }
            StandDLLOptions = StandDLLOptions.Append("\nBack").ToArray();
            Menus["StandDLL"] = StandDLLOptions;
        }

        public static void ReloadFileOptions()
        {
            StandFileOptions = new string[] { };
            string autoInject = Settings["autoInject"] .Equals("true") ? "Auto inject: enabled" : "Auto inject: disabled";
            
            StandFileOptions = StandFileOptions.Append(autoInject).ToArray();


            if (FolderExists.CheckFolderExists(StandFolder, false) != "null")
            {
                StandFileOptions = StandFileOptions.Append("\nDelete Stand Folder").ToArray();

                if (FolderExists.CheckFolderExists(StandBinFolder, false) != "null")
                {
                    StandFileOptions = StandFileOptions.Append("Delete all stand dlls\n").ToArray();
                    foreach (string version in SupportedStandVersions)
                    {
                        if (File.Exists(Path.Combine(StandBinFolder, $"Stand_{version}.dll")))
                        {
                            StandFileOptions = StandFileOptions.Append($"Delete Stand DLL ({version})").ToArray();
                        }
                    }
                }
            }


            StandFileOptions = StandFileOptions.Append("\nBack").ToArray();
            Menus["StandFile"] = StandFileOptions;
        }

    }
}