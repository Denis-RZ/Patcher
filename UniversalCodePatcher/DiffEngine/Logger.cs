namespace UniversalCodePatcher.DiffEngine
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public class LogEntry
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public System.DateTime Time { get; set; } = System.DateTime.Now;
    }

    public interface ILogger
    {
        void Log(LogLevel level, string message);
        System.Collections.Generic.List<LogEntry> Entries { get; }
    }

    public class ListLogger : ILogger
    {
        public System.Collections.Generic.List<LogEntry> Entries { get; } = new();
        public void Log(LogLevel level, string message)
        {
            Entries.Add(new LogEntry { Level = level, Message = message, Time = System.DateTime.Now });
        }
    }
}
