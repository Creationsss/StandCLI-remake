using System.Text.Json;
using System.Text.RegularExpressions;

namespace StandCLI.Handlers
{
    public class NetworkHandler
    {

        private static readonly string SupportedVersions = "https://stand.gg/stand-versions.txt";
        private static readonly string GithubApiLink = "https://api.github.com/repos/Creationsss/StandCLI-remake/releases";

        public static async Task<string[]> LatestGitCommit()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("User-Agent", "C# App");

            var response = await client.GetAsync(GithubApiLink);
            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<string>();
            }

            var stringResponse = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(stringResponse);
            JsonElement root = document.RootElement;
            JsonElement latestRelease = root[0];

            string TagName = latestRelease.GetProperty("tag_name").GetString() ?? string.Empty;
            if (TagName == Program.CurrentStandCLIVersion) return Array.Empty<string>();

            string rawChangeLog = latestRelease.GetProperty("body").GetString()?.Replace("\r\n", "\n").Replace("#", string.Empty).Replace("Changelog", string.Empty) ?? string.Empty;
            string ChangeLog = Regex.Replace(rawChangeLog, @"^\s+", "", RegexOptions.Multiline);

            JsonElement assetsArray = latestRelease.GetProperty("assets");
            string downloadUrl = string.Empty;

            for (int i = 0; i < assetsArray.GetArrayLength(); i++)
            {
                JsonElement asset = assetsArray[i];
                string? name = asset.GetProperty("name").GetString();
                if (name != null && name.EndsWith(".exe"))
                {
                    downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? string.Empty;
                    break;
                }
            }

            return new[] { TagName, ChangeLog, downloadUrl };
        }

        public static async Task<string[]> SupportedStandVersion()
        {
            using HttpClient client = new();
            try
            {
                HttpResponseMessage response = await client.GetAsync(SupportedVersions);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    string[] lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        lines[i] = lines[i].Trim();
                    }
                    return lines;
                }
                else
                {
                    return new[] { $"Error: {response.StatusCode}" };
                }
            }
            catch (Exception ex)
            {
                Program.logfile?.Log("Error while getting Supported stand versions." + ex);
                Console.WriteLine("Failed to get supported stand versions. Exiting...");
                Thread.Sleep(5000);
                Environment.Exit(1); // Return a non-zero exit code to indicate failure.
                return new[] { "Error" }; // This line will never be reached but is needed to satisfy the return type.
            }
        }

        public static async Task<bool> DownloadStandDll(string standDllVersion, string destinationPath, bool skipPrint = false)
        {
            string downloadUrl = $"https://stand.gg/Stand {standDllVersion}.dll";

            if (File.Exists(destinationPath))
            {
                if(skipPrint) return true;
                if (Program.UsingStandVersion() == standDllVersion)
                {
                    Console.WriteLine($"version {standDllVersion} is already in use");
                }
                else
                {
                    Console.WriteLine($"Setting version {standDllVersion} to be used\n");
                    Program.UsingStandVersion(standDllVersion, true);
                }
                return true;
            }

            try
            {
                string? directoryPath = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    FolderExists.CheckFolderExists(directoryPath, true);
                }

                using (HttpClient httpClient = new())
                {
                    if (!skipPrint)
                    {
                        Console.Clear();
                        Console.WriteLine($"Downloading version {standDllVersion}\n ");
                    }

                    using HttpResponseMessage response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    if (response.IsSuccessStatusCode)
                    {
                        long? totalFileSize = response.Content.Headers.ContentLength;

                        if (totalFileSize.HasValue)
                        {
                            using Stream contentStream = await response.Content.ReadAsStreamAsync();
                            using FileStream fileStream = File.Create(destinationPath + ".tmp");
                            var buffer = new byte[8192];
                            var bytesRead = 0;
                            var totalBytesRead = 0L;

                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                                var percentage = (double)totalBytesRead / totalFileSize.Value;

                                if (!skipPrint)
                                {
                                    Console.Write($"\rProgress: {percentage:P0}".PadRight(Console.WindowWidth - 1));
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nTotal file size is not available. Progress reporting disabled.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"\nFailed to download Stand DLL version {standDllVersion}. HTTP Status Code: {response.StatusCode}");
                        return false;
                    }
                }

                File.Move(destinationPath + ".tmp", destinationPath);

                FileInfo fileInfo = new(destinationPath);

                if (fileInfo.Length < 1024)
                {
                    File.Delete(destinationPath);
                    Console.WriteLine($"\nError: It appears that the DLL download for version {standDllVersion} has failed. Ensure there is no interference from antivirus software.");
                    return false;
                }

                if (!skipPrint)
                {
                    Console.WriteLine($"\nDownloaded version {standDllVersion} to {destinationPath}\n");
                    Program.logfile?.Log($"Succesfully installed Stand {standDllVersion}!");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError downloading Stand DLL: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Program.logfile?.Log($"\nError downloading Stand DLL: {ex.Message}");
                Program.logfile?.Log($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
