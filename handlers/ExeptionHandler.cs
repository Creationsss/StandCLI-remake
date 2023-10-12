using System.Text.Json;
using PostSharp.Aspects;

namespace StandCLI.Handlers
{
    [Serializable]
    public sealed class CatchAndLogAttribute : OnMethodBoundaryAspect
    {
        public override void OnException(MethodExecutionArgs args)
        {
            string methodName = args.Exception.Message;
            ExceptionHandler.HandleException(methodName, args.Exception);
            args.FlowBehavior = FlowBehavior.Continue;
        }
    }

    public class ExceptionHandler
    {
        public static async Task InitializeErrorHandles()
        {
            if (Program.ErrorHandles == null || Program.ErrorHandles.Count == 0)
            {
                Program.ErrorHandles = await NetworkHandler.ExceptionsList();
            }
        }

        public static bool DoesExceptionExist(string exceptionKey)
        {
            return Program.ErrorHandles?.ContainsKey(exceptionKey) == true;
        }

        private static string GetNestedValue(string exceptionKey, string nestedKey)
        {
            if (!DoesExceptionExist(exceptionKey)) 
                return string.Empty;

            var nestedJson = Program.ErrorHandles[exceptionKey]?.ToString() ?? string.Empty;
            return DeserializeNestedJson(nestedJson, nestedKey);
        }

        private static string DeserializeNestedJson(string nestedJson, string nestedKey)
        {
            var nestedDict = JsonSerializer.Deserialize<Dictionary<string, object>>(nestedJson) ?? new Dictionary<string, object>();
            return nestedDict.ContainsKey(nestedKey) ? nestedDict[nestedKey]?.ToString() ?? string.Empty : string.Empty;
        }

        public static void HandleException(string exceptionKey, Exception ex)
        {
            string consoleMessage = GetNestedValue(exceptionKey, "ConsoleMessage");
            string logMessage = GetNestedValue(exceptionKey, "LogMessage");

            Console.Clear();
            Console.WriteLine(exceptionKey + "\n");
            
            if (!string.IsNullOrEmpty(consoleMessage) && !string.IsNullOrEmpty(logMessage))
            {
                Console.WriteLine(consoleMessage);
                Program.logfile?.Log(logMessage);
            }
            else
            {
                Console.WriteLine($"We sadly couldnt catch this exception, please report this :)\n\n{ex}");
                Program.logfile?.Log($"We sadly couldnt catch this exception, please report this :)\n\n{ex}");
            }

            Console.ReadKey();
        }
    }
}
