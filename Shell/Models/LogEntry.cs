using System;

namespace Shell.Models
{
    public enum LogLevel { Info, Warning, Error, Success }

    public class LogEntry
    {
        public DateTime Timestamp { get; init; } = DateTime.Now;
        public LogLevel Level { get; init; } = LogLevel.Info;
        public string Source { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string TimeText => Timestamp.ToString("HH:mm:ss.fff");

        public string LevelText => Level switch
        {
            LogLevel.Info => "\u2139",
            LogLevel.Warning => "\u26A0",
            LogLevel.Error => "\u2716",
            LogLevel.Success => "\u2713",
            _ => "\u00B7"
        };

        public string DisplayText => $"[{TimeText}] {LevelText} [{Source}] {Message}";
    }
}
