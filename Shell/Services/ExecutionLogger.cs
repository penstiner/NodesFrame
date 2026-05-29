using System;
using System.Collections.ObjectModel;
using System.Windows;
using Shell.Models;

namespace Shell.Services
{
    public static class ExecutionLogger
    {
        public static ObservableCollection<LogEntry> Logs { get; } = new();
        public static event Action<LogEntry>? LogAdded;

        public static void Info(string source, string message) =>
            Add(LogLevel.Info, source, message);
        public static void Warning(string source, string message) =>
            Add(LogLevel.Warning, source, message);
        public static void Error(string source, string message) =>
            Add(LogLevel.Error, source, message);
        public static void Success(string source, string message) =>
            Add(LogLevel.Success, source, message);
        public static void Clear()
        {
            if (Application.Current?.Dispatcher.CheckAccess() == true)
                Logs.Clear();
            else
                Application.Current?.Dispatcher.Invoke(() => Logs.Clear());
        }

        private static void Add(LogLevel level, string source, string message)
        {
            var entry = new LogEntry { Level = level, Source = source, Message = message };

            // 确保在 UI 线程操作集合
            if (Application.Current?.Dispatcher.CheckAccess() == true)
            {
                Logs.Add(entry);
                LogAdded?.Invoke(entry);
            }
            else
            {
                Application.Current?.Dispatcher.BeginInvoke(() =>
                {
                    Logs.Add(entry);
                    LogAdded?.Invoke(entry);
                });
            }
        }
    }
}
