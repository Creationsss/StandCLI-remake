using System.Diagnostics;

namespace StandCLI.Handlers
{
    class UpdateHandler 
    {

        public static async Task<Tuple<bool, string>> CheckForUpdate()
        {
            string[] result = await NetworkHandler.LatestGitCommit();
            if(result == Array.Empty<string>()) return Tuple.Create(false, String.Empty);

            if(Program.Settings["updateCheck"] == "false") return Tuple.Create(false, string.Empty);
            if(double.Parse(result[0]) > double.Parse(Program.CurrentStandCLIVersion))
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(result[1]);
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine($"\nWould you like to update to version {result[0]}\n(y to confirm or any other key to cancel)");
                string input = Console.ReadKey(intercept: true).KeyChar.ToString().ToLower();

                if(input == "y")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n\nUpdating...");
                    Console.ResetColor();
                    return Tuple.Create(true, result[2]);
                }
            }
            return Tuple.Create(false, string.Empty);
        }

        public static bool Updater(string DownloadLink)
        {
            try
            {
                using HttpClient client = new();
                var data = client.GetByteArrayAsync(DownloadLink).Result;
                string? currentExePath = Process.GetCurrentProcess().MainModule?.FileName;

                if (currentExePath == null)
                {
                    Console.WriteLine("Could not get the current executable path.");
                    return false;
                }

                string tempFile = Path.Combine(Path.GetTempPath(), "new_version.exe");
                File.WriteAllBytes(tempFile, data);

                string batchCommands = $"@echo off\n" +
                                        "timeout /t 3\n" +
                                        $"del /F \"{currentExePath}\"\n" +
                                        $"move \"{tempFile}\" \"{currentExePath}\"\n" +
                                        $"start {currentExePath}";

                string batchFilePath = Path.Combine(Path.GetTempPath(), "update.bat");
                File.WriteAllText(batchFilePath, batchCommands);

                Process.Start(new ProcessStartInfo()
                {
                    FileName = batchFilePath,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                Environment.Exit(0);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return false;
            }
        }
    }
}