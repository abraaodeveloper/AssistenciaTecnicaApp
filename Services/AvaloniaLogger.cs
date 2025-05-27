using System;
using Serilog;

namespace AssistenciaTecnicaApp.Services
{
    public class AvaloniaLogger : ILogger
    {
        private readonly string _source;

        public AvaloniaLogger(string source = "AssistenciaTecnicaApp")
        {
            _source = source;
            Log.Debug("=== Logger Initialized ===");
        }

        public void Debug(string message)
        {
            Log.Debug("{Source}: {Message}", _source, message);
        }

        public void Information(string message)
        {
            Log.Information("{Source}: {Message}", _source, message);
        }

        public void Warning(string message)
        {
            Log.Warning("{Source}: {Message}", _source, message);
        }

        public void Error(string message)
        {
            Log.Error("{Source}: {Message}", _source, message);
        }

        public void Error(Exception ex, string message)
        {
            Log.Error(ex, "{Source}: {Message}", _source, message);
        }

        public void Fatal(Exception ex, string message)
        {
            Log.Fatal(ex, "{Source}: {Message}", _source, message);
            if (ex.InnerException != null)
            {
                Log.Fatal(ex.InnerException, "{Source}: Inner Exception", _source);
            }
        }
    }
} 