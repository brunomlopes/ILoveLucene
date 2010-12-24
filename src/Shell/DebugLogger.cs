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
            Debug.WriteLine(string.Format(format, args));
        }

        public void Warn(string format, params object[] args)
        {
            Debug.WriteLine(string.Format(format, args));
        }

        public void Error(Exception exception)
        {
            Debug.WriteLine(exception.ToString());
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            Debug.WriteLine(string.Format(format,args));
            Debug.WriteLine(exception.ToString());
            Debug.WriteLine(exception.StackTrace);
        }
    }
}