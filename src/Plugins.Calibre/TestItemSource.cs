using System;
using Core.Abstractions;
using Xunit;
using System.Linq;

namespace Plugins.Calibre
{
    public class TestItemSource
    {
        [Fact]
        public void CanReturnBooksInMyPc()
        {
            var source = new CalibreBookSource();
            source.Log = new MockLog();

            source.Config = new Configuration();

            var items = source.GetItems().Result.ToList();
            Assert.NotEqual(0, items.Count);
        }
        
    }

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