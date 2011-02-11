using System;
using System.Diagnostics;
using CoreILog = Core.Abstractions.ILog;
using ILog = Caliburn.Micro.ILog;

namespace ILoveLucene
{
    public class DebugLogger : ILog, CoreILog
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