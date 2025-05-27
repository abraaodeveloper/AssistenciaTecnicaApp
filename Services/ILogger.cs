using System;

namespace AssistenciaTecnicaApp.Services
{
    public interface ILogger
    {
        void Debug(string message);
        void Information(string message);
        void Warning(string message);
        void Error(string message);
        void Error(Exception ex, string message);
        void Fatal(Exception ex, string message);
    }
} 