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
        readonly Dictionary<string, string> ErrorHandles = new();
        


    }
}