using System;
using System.Diagnostics;
using Caliburn.Micro;

namespace ILoveLucene
{
    public class DebugLogger : ILog
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
    }
}