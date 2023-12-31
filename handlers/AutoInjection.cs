namespace StandCLI.Handlers
{
    class AutoInjection
    {
        public static async Task AutoInject()
        {
            while (true)
            {
                if (!IsInjectionNeeded())
                {
                    return;
                }

                int pid = InjectMethods.GetGtaPid();
                
                if (pid != -1)
                {
                    string? delayValue = Program.IniFile?.ReadValue("Settings", "autoInjectDelay");
                    int InjectDelay = delayValue != null ? int.Parse(delayValue) : 0;
                    
                    if (InjectDelay == 0)
                    {
                        Program.logfile?.Log("Inject delay is invalid! Resetting.");
                        InjectDelay = 45000;
                        Program.IniFile?.SetValue("Settings", "autoInjectDelay", "45000");
                    }

                    for (int remainingMilliseconds = InjectDelay; remainingMilliseconds >= 0; remainingMilliseconds -= 1000)
                    {
                        if (!IsInjectionNeeded())
                        {
                            return;
                        }
                        string? standVersion = Program.IniFile?.ReadValue("Settings", "standVersion");
                        if(string.IsNullOrEmpty(standVersion))
                        {
                            Program.IniFile?.SetValue("Settings", "standVersion", Program.CurrentStandCLIVersion);
                        }
                        int remainingSeconds = remainingMilliseconds / 1000;
                        Console.Title = $"(Auto Inject) Injecting version {standVersion} in {remainingSeconds}s";
                        await Task.Delay(1000);
                    }

                    InjectMethods.Inject();
                    Console.Title = "(Auto Inject) Injected version " + Program.IniFile?.ReadValue("Settings", "standVersion");
                    await Task.Delay(5000);
                    ResetTitle();
                    return;
                }
                else
                {
                    Console.Title = "(Auto Inject) Waiting for GTA5";
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        private static void ResetTitle()
        {
            Console.Title = "Stand CLI Version " + Program.CurrentStandCLIVersion;
        }

        private static bool IsInjectionNeeded()
        {
            string autoInjectSetting = Program.Settings["autoInject"];
            string? autoInjectINI = Program.IniFile?.ReadValue("Settings", "autoInject");

            if (autoInjectINI == "false" || autoInjectSetting == "false" || Program.injected)
            {
                ResetTitle();
                return false;
            }
            return true;
        }
    }
}
