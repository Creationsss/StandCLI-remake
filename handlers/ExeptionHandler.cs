namespace StandCLI.handlers
{

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class CatchAndLogAttribute : Attribute
    {
        public CatchAndLogAttribute()
        {
        }
    }

    public class ExeptionHandler
    {
        public static async Task InitializeErrorHandles()
        {
            if (Program.ErrorHandles.Count == 0)
            {
                Program.ErrorHandles = await NetworkHandler.ExceptionsList();
            }
        }

        public static bool DoesExceptionExist(string exceptionKey)
        {
            return Program.ErrorHandles.ContainsKey(exceptionKey);
        }


        public ExeptionHandler()
        {
            
        }

    }
}