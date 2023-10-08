using System.Diagnostics;

namespace StandCLI.handlers
{
    class RuntimeHandler
    {
        private static readonly Stopwatch stopwatch = new();

        public static void StartElapsedTime() 
        {
            stopwatch.Start();
        }

        public static void StopElapsedTime() 
        {
            Stopwatch stopwatch = new();
            stopwatch.Stop();
        }

        public static string GetElapsedTime() 
        {
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = string.Format("{0:00}h:{1:00}m:{2:00}s.{3:00}ms", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            return elapsedTime;
        }
    }
}