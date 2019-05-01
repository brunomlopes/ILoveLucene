using System;
using Core.Abstractions;

namespace Plugins.Calibre.Tests
{
    internal class MockLog : ILog
    {
        public void Info(string format, params object[] args)
        {
            Debug(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Debug(format, args);
        }

        public void Error(Exception exception)
        {
            throw exception;
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            throw exception;
        }

        public void Debug(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(format, args);
        }
    }
}