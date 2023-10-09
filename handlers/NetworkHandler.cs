namespace StandCLI.handlers
{
    public class NetworkHandler
    {
        public static async Task<string[]> SupportedStandVersion()
        {
            string url = "https://creations.works/assets/StandCLI_Supported.txt";

            using (HttpClient client = new())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        string[] lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        return lines;
                    }
                    else
                    {
                        return response.StatusCode.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    }
                }
                catch (Exception ex)
                {
                    Program.logfile?.Log("Error while getting Supported stand versions." + ex);
                    return ex.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
                }
            }
        }
        public static async Task<string[]?> GetLatestStandVersion()
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                string url = "https://stand.gg/versions.txt";
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    string[] versionInfo = responseContent.Split(':');

                    if (versionInfo.Length >= 2)
                    {
                        string standFullVersion = versionInfo[0].Trim();
                        string standDllVersion = versionInfo[1].Trim();

                        string? IniVersion = Program.IniFile?.ReadValue("Settings", "standVersion");

                        if(IniVersion == "" || IniVersion == null)
                        {
                            Program.IniFile?.SetValue("Settings", "standVersion", standDllVersion);
                        }
                        return new string[] { standFullVersion, standDllVersion };
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid version data format");
                    }
                }
                else
                {
                    Program.logfile?.Log($"HTTP Error: {response.StatusCode}");
                    Console.WriteLine($"HTTP Error: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HttpRequestException: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return null;
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

                using (HttpClient httpClient = new HttpClient())
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
                            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                            {
                                using (FileStream fileStream = File.Create(destinationPath + ".tmp"))
                                {
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
                Console.WriteLine($"Error downloading Stand DLL: {ex.ToString()}");
                Program.logfile?.Log($"\nError downloading Stand DLL: {ex.Message}");
                Program.logfile?.Log($"Stack Trace: {ex.StackTrace}");
                Program.logfile?.Log($"Error downloading Stand DLL: {ex.ToString()}");
                return false;
            }
        }
    }
}
