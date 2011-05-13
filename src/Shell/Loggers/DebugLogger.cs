using System;
using Caliburn.Micro;

namespace ILoveLucene.Loggers
{
    public class DebugLogger : ILog, Core.Abstractions.ILog
    {
        public void Info(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args));
        }

        public void Warn(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args));
        }

        public void Error(Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception.ToString());
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args));
            System.Diagnostics.Debug.WriteLine(exception.ToString());
            System.Diagnostics.Debug.WriteLine(exception.StackTrace);
        }

        public void Debug(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(format, args);
        }
    }
}