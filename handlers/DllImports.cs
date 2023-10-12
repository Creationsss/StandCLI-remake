using System.Runtime.InteropServices;

namespace StandCLI.Handlers
{
    class DLLImports
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

        [DllImport("ntdll.dll")]
        private static extern IntPtr MyGetModuleHandle(string moduleName);

        [DllImport("ntdll.dll")]
        private static extern IntPtr MyGetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();


        public static IntPtr OpenProcessWrapper(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId)
        {
            return OpenProcess(dwDesiredAccess, bInheritHandle, dwProcessId);
        }

        public static IntPtr MyGetModuleHandleWrapper(string moduleName)
        {
            return MyGetModuleHandle(moduleName);
        }

        public static IntPtr MyGetProcAddressWrapper(IntPtr hModule, string procName)
        {
            return MyGetProcAddress(hModule, procName);
        }

        public static IntPtr GetProcAddressWrapper(IntPtr hModule, string lpProcName)
        {
            return GetProcAddress(hModule, lpProcName);
        }

        public static IntPtr GetModuleHandleWrapper(string lpModuleName)
        {
            return GetModuleHandle(lpModuleName);
        }

        public static int WriteProcessMemoryWrapper(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten)
        {
            return WriteProcessMemory(hProcess, lpBaseAddress, buffer, size, lpNumberOfBytesWritten);
        }

        public static IntPtr CreateRemoteThreadWrapper(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId)
        {
            return CreateRemoteThread(hProcess, lpThreadAttribute, dwStackSize, lpStartAddress, lpParameter, dwCreationFlags, lpThreadId);
        }

        public static int CloseHandleWrapper(IntPtr hObject)
        {
            return CloseHandle(hObject);
        }

        public static IntPtr VirtualAllocExWrapper(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect)
        {
            return VirtualAllocEx(hProcess, lpAddress, dwSize, flAllocationType, flProtect);
        }
    }
}
