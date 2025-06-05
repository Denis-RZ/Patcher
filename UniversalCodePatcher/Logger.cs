using System;

namespace UniversalCodePatcher
{
    public enum LogLevel { Info, Warning, Error }

    public class LogEntry
    {
        public LogLevel Level { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }
        public LogEntry(LogLevel level, string message)
        {
            Level = level;
            Message = message;
            Timestamp = DateTime.Now;
        }
    }

    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        event Action<LogEntry> OnLogged;
    }

    public class SimpleLogger : ILogger
    {
        public event Action<LogEntry>? OnLogged;
        public void LogInfo(string message) => Log(LogLevel.Info, message);
        public void LogWarning(string message) => Log(LogLevel.Warning, message);
        public void LogError(string message) => Log(LogLevel.Error, message);

        private void Log(LogLevel level, string message)
        {
            var entry = new LogEntry(level, message);
            OnLogged?.Invoke(entry);
        }
    }
}
