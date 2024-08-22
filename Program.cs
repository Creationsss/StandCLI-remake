using StandCLI.Handlers;

namespace StandCLI
{
    class Program
    {
        public static IniFile? IniFile;
        public static Dictionary<string, string> Settings = new();
        private static readonly Dictionary<string, string> DefaultSettings = new()
        {
            {"disclaimer", "true"},
            {"autoInject", "false"},
            {"confirmOptions", "true"},
            {"autoInjectDelay", "45000"},
            {"updateCheck", "true"}
        };

        private static string[] MainMenuOptions = Array.Empty<string>();
        private static string[] StandFileOptions = Array.Empty<string>();
        private static string[] StandDLLOptions = Array.Empty<string>();

        private static Dictionary<string, object> Menus = new();

        public static string StandFolder = FolderExists.CheckFolderExists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand"), false);
        public static string StandBinFolder = FolderExists.CheckFolderExists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand", "Bin"), false);
        public static string DocumentsFolder = FolderExists.CheckFolderExists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), false);
        public static string StandCLIFolder = FolderExists.CheckFolderExists(Path.Combine(DocumentsFolder, "StandCLI"));
        public static string StandCLILogFolder = FolderExists.CheckFolderExists(Path.Combine(StandCLIFolder, "logs"));

        public static string[] SupportedStandVersions = Array.Empty<string>();
        public static string CurrentStandDllVersion = string.Empty;
        public static string CurrentStandCLIVersion = "2.2";
        
        public static bool injected = false;
        public static Logger? logfile;

        static void Main(string[] args)
        {
            RuntimeHandler.StartElapsedTime();

            IniFile = new(Path.Combine(StandCLIFolder, "settings.ini"));

            string dateTime = DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
            logfile = new(Path.Combine(StandCLILogFolder, $"{dateTime}.log"));
            
            LauncherCreation.RunningAsLauncher();
            
            CheckSettings();

            SupportedStandVersions = NetworkHandler.SupportedStandVersion().Result;
            Task<Tuple<bool, string>> task = Task.Run(() => UpdateHandler.CheckForUpdate());
            
            task.Wait();
            bool result = task.Result.Item1;

            if(result) UpdateHandler.Updater(task.Result.Item2);

            CurrentStandDllVersion = SupportedStandVersions[0];

            logfile.Log("StandCLI " + CurrentStandCLIVersion + " Reporting for duty!");

            if (!string.IsNullOrEmpty(StandCLIFolder)) logfile.Log("StandCLI's goodies will be stored on: " + StandCLIFolder);
            if (string.IsNullOrEmpty(StandFolder)) logfile.Log("Stand's goodies are in: " + StandFolder);
            if (!string.IsNullOrEmpty(StandBinFolder)) logfile.Log("Stand's DLLs will be stored in: " + StandBinFolder);

            CheckWindowsVersion();
            Disclaimer();
            SetMenuOptions();

            Task.Run(() => AutoInjection.AutoInject());

            MenuOptionsHandler.MenuOptions("MainMenu");
        }

        public static string CheckWindowsVersion()
        {
            OperatingSystem SystemVersion = Environment.OSVersion;

            var version = SystemVersion.Version;
            var cutver = version.ToString()[..4];

            if(cutver != "10.0")
            {
                logfile?.Log("Windows version " + cutver + " is ancient! there might be issues using it, https://www.itconvergence.com/blog/risks-of-using-outdated-operating-system/");
            }
            return cutver;
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
                if(string.IsNullOrEmpty(stand_dll_ver))
                {
                    return CurrentStandDllVersion;
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
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\nKnown problems:");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("> Injecting before the game starts loading will cause error ERR_GFX_D3D_INIT.");
            Console.WriteLine("> AutoInject will crash you if you are still on the main menu options screen when it tries to inject");
            Console.WriteLine("> Joining public sessions through RID seems to be unstable.");
            Console.WriteLine("> General Stability issues.");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\nUse the arrow keys to scroll and enter to select an option");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press c to never see this again or press any other key to continue");
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
                "Launcher Options",
                "Other Settings",
                "\nExit"
            };

            ReloadAllOptions();

            Menus.Add("MainMenu", MainMenuOptions);
        }

        public static Dictionary<string, object> ReturnMenus()
        {
            return Menus;
        }

        public static void ReloadStandDLLMenuOptions()
        {
            StandDLLOptions = Array.Empty<string>();
            Dictionary<string, List<string>> versionTextMap = new();

            foreach (string version in SupportedStandVersions)
            {
                if (!versionTextMap.ContainsKey(version))
                {
                    versionTextMap[version] = new List<string>();
                }

                if (version == CurrentStandDllVersion) versionTextMap[version].Insert(0, "latest");

                if (File.Exists(Path.Combine(StandBinFolder, $"Stand_{version}.dll")))
                {
                    if (UsingStandVersion() == version  && !versionTextMap[version].Contains("using")) versionTextMap[version].Insert(0, "using");
                    versionTextMap[version].Insert(0, "installed");
                }
            }

            if (StandBinFolder != "null" && Directory.Exists(StandBinFolder))
            {
                foreach (FileInfo file in new DirectoryInfo(StandBinFolder).GetFiles())
                {
                    if (file.Name.StartsWith("Stand_") && file.Name.EndsWith(".dll"))
                    {
                        string[] splitVersion = file.Name.Replace("Stand_", "").Replace(".dll", "").Split('.');
                        string localVersion = string.Join(".", splitVersion);

                        if(versionTextMap.ContainsKey(localVersion)) continue;

                        if (!versionTextMap.ContainsKey(localVersion))
                        {
                            versionTextMap[localVersion] = new List<string>();
                        }

                        if (UsingStandVersion() == localVersion  && !versionTextMap[localVersion].Contains("using")) 
                        {
                            versionTextMap[localVersion].Insert(0, "using");
                        }

                        versionTextMap[localVersion].Add("installed");

                        if (!SupportedStandVersions.Contains(localVersion))
                        {
                            versionTextMap[localVersion].Add("outdated");
                        }
                    }
                }
            }

            foreach (var entry in versionTextMap)
            {
                string version = entry.Key;
                List<string> addText = entry.Value;
                string formattedText = string.Join(", ", addText);
                if (!string.IsNullOrEmpty(formattedText))
                {
                    formattedText = "(" + formattedText + ")";
                }
                StandDLLOptions = StandDLLOptions.Append($"{version} {formattedText}").ToArray();
            }

            StandDLLOptions = StandDLLOptions.Append("\nBack").ToArray();
            Menus["StandDLL"] = StandDLLOptions;
        }

        public static void ReloadFileOptions()
        {
            StandFileOptions = Array.Empty<string>();

            string autoInject = Settings["autoInject"] .Equals("true") ? "Auto inject: enabled" : "Auto inject: disabled";
            StandFileOptions = StandFileOptions.Append(autoInject).ToArray();

            string? ShowDisclaimer = (IniFile?.ReadValue("Settings", "disclaimer")?.Equals("true") ?? false) ? "Show disclaimer: enabled" : "Show disclaimer: disabled";
            StandFileOptions = StandFileOptions.Append(ShowDisclaimer).ToArray();

            string? UpdateCheck = (IniFile?.ReadValue("Settings", "updateCheck")?.Equals("true") ?? false) ? "Update check: enabled" : "Update check: disabled";
            StandFileOptions = StandFileOptions.Append(UpdateCheck).ToArray();

            string? ConfirmOptions = (IniFile?.ReadValue("Settings", "confirmOptions")?.Equals("true") ?? false) ? "Confirm options: enabled\n" : "Confirm options: disabled\n";
            StandFileOptions = StandFileOptions.Append(ConfirmOptions).ToArray();

            string? autoInjectDelay = IniFile?.ReadValue("Settings", "autoInjectDelay") ?? null;
            if (autoInjectDelay != null)
            {
                StandFileOptions = StandFileOptions.Append($"Auto inject delay: {autoInjectDelay}ms\n").ToArray();
            }
            else
            {
                StandFileOptions = StandFileOptions.Append("Auto inject delay: 45000ms\n").ToArray();
            }

            string? gtaPath = IniFile?.ReadValue("Settings", "gtaPath") ?? null;
            if (gtaPath != null)
            {
                StandFileOptions = StandFileOptions.Append($"GTA Path: {gtaPath}").ToArray();
            }
            else
            {
                StandFileOptions = StandFileOptions.Append("GTA Path: not set").ToArray();
            }

            if(Directory.Exists(StandCLIFolder)) StandFileOptions = StandFileOptions.Append($"StandCLI Logs and Settings: {StandCLIFolder}").ToArray();
            if(Directory.Exists(StandFolder)) StandFileOptions = StandFileOptions.Append($"Stand Folder: {StandFolder}").ToArray();
            
            if (FolderExists.CheckFolderExists(StandFolder, false) != "null")
            {
                StandFileOptions = StandFileOptions.Append("\nDelete Stand Folder").ToArray();

                if(FolderExists.CheckFolderExists(Path.Combine(StandBinFolder, "Temp"), false) != "null")
                {
                    StandFileOptions = StandFileOptions.Append("Delete Stand Temp Folder").ToArray();
                }

                int length = Directory.GetFiles(StandBinFolder).Length;
                if (FolderExists.CheckFolderExists(StandBinFolder, false) != "null" && length > 0)
                {
                    StandFileOptions = StandFileOptions.Append("Delete All stand versions\n").ToArray();
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

        public static void ReloadLauncherOptions()
        {
            string? launcherPath = IniFile?.ReadValue("Settings", "launcherPath") ?? null;
            string? gtaPath = IniFile?.ReadValue("Settings", "gtaPath") ?? null;

            string[] LauncherOptions = Array.Empty<string>();

            if(!LauncherCreation.CheckIfLauncherExists())
            {
                LauncherOptions = LauncherOptions.Append("Create Launcher").ToArray();
            }
            else
            {
                if(gtaPath != null || gtaPath != String.Empty && LauncherCreation.CheckIfLauncherExists()) LauncherOptions = LauncherOptions.Append($"Launcher Path: {launcherPath}\n").ToArray();
                LauncherOptions = LauncherOptions.Append("Reinstall Launcher").ToArray();
                LauncherOptions = LauncherOptions.Append("Delete Launcher").ToArray();
                
            }

            LauncherOptions = LauncherOptions.Append("\nBack").ToArray();
            Menus["LauncherOptions"] = LauncherOptions;
        }

        public static bool ReloadAllOptions()
        {
            try
            {
                ReloadFileOptions();
                ReloadStandDLLMenuOptions();
                ReloadLauncherOptions();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}