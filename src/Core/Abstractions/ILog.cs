using System;

namespace Core.Abstractions
{
    public interface ILog
    {
        void Info(string format, params object[] args);
        void Warn(string format, params object[] args);
        void Error(Exception exception);
        void Error(Exception exception, string format, params object[] args);
        void Debug(string format, params object[] args);
    }
}