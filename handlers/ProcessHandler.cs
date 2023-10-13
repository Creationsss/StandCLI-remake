using System.Diagnostics;

namespace StandCLI.Handlers
{
    class GtaProcHandler
    {
        public static void AutoResetInjection(int gta_pid)
        {
            if(gta_pid == -1) return;

            try{
                Process process = Process.GetProcessById(gta_pid);
                process.EnableRaisingEvents = true;
                process.Exited += new EventHandler(Process_Exited);
            }
            catch
            {
                Program.logfile?.Log("Unexpected error while resetting injected status");
            }
        }

        private static void Process_Exited(object? sender, EventArgs e)
        {
            Program.injected = false;
            Task.Delay(2000).Wait();
            Task.Run(() => AutoInjection.AutoInject());
            Program.logfile?.Log("GTA5 process exited, restarting auto-injection // injection status");
        }
    }
}