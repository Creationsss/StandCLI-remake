using System.Diagnostics;
using System.Text;
using System.Security.Cryptography;

namespace StandCLI.handlers
{
    class InjectMethods 
    {
        private static Random random = new();

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder randomString = new(length);

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] randomNumberBuffer = new byte[4];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(randomNumberBuffer);
                    uint randomIndex = BitConverter.ToUInt32(randomNumberBuffer, 0) % (uint)chars.Length;
                    randomString.Append(chars[(int)randomIndex]);
                }
            }

            return randomString.ToString();
        }


        public static int GetGtaPid()
        {
            int gta_pid = -1;
            Process[] processes = Process.GetProcessesByName("GTA5");

            if (processes.Length > 0)
            {
                Process gtaProcess = processes[0];
                gta_pid = gtaProcess.Id;
            }

            return gta_pid;
        }

        public static unsafe string Inject()
        {
            int gta_pid = GetGtaPid();
            string stand_dll;
            string? usingVers = Program.UsingStandVersion();
            string stand_vers = "";
            
            if (gta_pid == -1)
            {
                Program.logfile?.Log("Could not find GTA5.EXE");
                return "Couldn't find GTA5 process";
            }

            IntPtr hProcess = DLLImports.OpenProcessWrapper(1082u, 1, (uint)gta_pid);
            if (hProcess != IntPtr.Zero)
            {
                try
                {
                    IntPtr loadLibraryAddress = DLLImports.GetProcAddressWrapper(DLLImports.GetModuleHandleWrapper("kernel32.dll"), "LoadLibraryW");
                    if (loadLibraryAddress == IntPtr.Zero)
                    {
                        return "Couldn't find LoadLibraryW address";
                    }

                    string temp_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand", "Bin", "Temp");
                    temp_folder = FolderExists.CheckFolderExists(temp_folder, true);
                    string download_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand", "Bin");
                    download_folder = FolderExists.CheckFolderExists(download_folder, true);

                    if(download_folder == "null")
                    {
                        return "Couldn't create download folder";
                    }
                    else
                    {
                        if (!File.Exists(Path.Combine(download_folder, $"Stand_{usingVers}.dll")))
                        {
                            Task<bool> downloadTask = NetworkHandler.DownloadStandDll(Program.CurrentStandDllVersion, Path.Combine(download_folder, $"Stand_{Program.CurrentStandDllVersion}.dll"), true);
                            downloadTask.Wait();

                            Program.UsingStandVersion(Program.CurrentStandDllVersion, true);
                            Program.ReloadFileOptions();
                            Program.ReloadStandDLLMenuOptions();
                            stand_dll = Path.Combine(download_folder, $"Stand_{Program.CurrentStandDllVersion}.dll");
                            stand_vers = Program.CurrentStandDllVersion;
                        }
                        else {
                            stand_dll = Path.Combine(download_folder, $"Stand_{usingVers}.dll");
                            if(usingVers != null)
                            {
                                stand_vers = usingVers;
                            }

                        }
                    }
                    
                    string dllRandomized = Path.Combine(temp_folder, "CLI_" + GenerateRandomString(7) + ".dll");
                    File.Copy(stand_dll, dllRandomized);

                    byte[] bytes = Encoding.Unicode.GetBytes(dllRandomized);

                    IntPtr allocatedMemory = DLLImports.VirtualAllocExWrapper(hProcess, (IntPtr)(void*)null, (IntPtr)bytes.Length, 12288u, 64u);

                    if (allocatedMemory == IntPtr.Zero)
                    {
                        return "Failed to allocate memory in the game's process.";
                    }

                    if (DLLImports.WriteProcessMemoryWrapper(hProcess, allocatedMemory, bytes, (uint)bytes.Length, 0) == 0)
                    {
                        return "Couldn't write to allocated memory.";
                    }

                    IntPtr remoteThread = DLLImports.CreateRemoteThreadWrapper(hProcess, IntPtr.Zero, IntPtr.Zero, loadLibraryAddress, allocatedMemory, 0, IntPtr.Zero);

                    if (remoteThread == IntPtr.Zero)
                    {
                        return "Failed to create a remote thread for " + stand_dll;
                    }

                    Program.injected = true;
                    return $"Injected version {stand_vers}";
                }
                finally
                {
                    DLLImports.CloseHandleWrapper(hProcess);
                }
            }
            else
            {
                return "Failed to get a handle to the game's process.";
            }
        }
    }
}