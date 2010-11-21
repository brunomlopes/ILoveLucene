using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;

namespace ILoveLucene
{
    public class DebugLogger : ILog
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
    }
}