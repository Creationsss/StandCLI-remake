using System.Text.Json;
using StandCLI.Handlers;

namespace StandCLI.Handlers
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class CatchAndLogAttribute : Attribute
    {
        public CatchAndLogAttribute() { }
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
            
            if (!string.IsNullOrEmpty(consoleMessage))
            {
                Console.WriteLine(consoleMessage);
            }
            else
            {
                Console.WriteLine($"We sadly couldnt catch this exception, please report this :)\n{ex}");
            }
            Console.ReadKey();
        }
    }
}
