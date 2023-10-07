
namespace StandCLI.handlers 
{
    public class FolderExists
    {
        public static string CheckFolderExists(string folderPath, bool Create = true)
        {
            if (Directory.Exists(folderPath))
            {
                return folderPath;
            }
            else
            {
                if (!Create) return "null";
                Directory.CreateDirectory(folderPath);
                return folderPath;
            }
        }
    }

    public class FileExists
    {
        public static bool CheckFileExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                return true;
            }
            return false;
        }
    }

    public class IniFile
    {
        private readonly string filePath;

        public IniFile(string filePath)
        {
            this.filePath = filePath;
        }

        public void SetValue(string section, string key, string value)
        {
            List<string> lines = new List<string>();
            if (File.Exists(filePath))
            {
                lines = new List<string>(File.ReadAllLines(filePath));
            }

            bool sectionFound = false;
            bool keyFound = false;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("[" + section + "]"))
                {
                    sectionFound = true;
                    continue;
                }

                if (sectionFound && line.StartsWith(key + "="))
                {
                    lines[i] = key + "=" + value;
                    keyFound = true;
                    break;
                }
            }

            if (!sectionFound)
            {
                lines.Add("[" + section + "]");
            }

            if (!keyFound)
            {
                lines.Add(key + "=" + value);
            }

            File.WriteAllLines(filePath, lines);
        }

        public string? ReadValue(string section, string key)
        {
            List<string> lines = new List<string>();
            if (File.Exists(filePath))
            {
                lines = new List<string>(File.ReadAllLines(filePath));
            }

            bool sectionFound = false;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("[" + section + "]"))
                {
                    sectionFound = true;
                    continue;
                }

                if (sectionFound && line.StartsWith(key + "="))
                {
                    return line[(key.Length + 1)..].Trim();
                }
            }

            return null;
        }
    }

    public class Logger
    {
        public string logFilePath;

        public Logger(string logFileName)
        {
            this.logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);
            Console.WriteLine($"Logging to {logFilePath}");
        }

        public void Log(string message)
        {
            try
            {
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while logging: {ex.Message}");
            }
        }
    }
}