using System;
using System.IO;
using System.Text;
using Caliburn.Micro;
using CoreILog = Core.Abstractions.ILog;

namespace ILoveLucene
{
    public class FileLogger : ILog, CoreILog
    {
        private static readonly object SyncFile = new object();

        private readonly FileInfo _outFile;
        private string _tag;

        public FileLogger(FileInfo outFile, string tag = null)
        {
            if (tag == null) tag = string.Empty;
            _tag = tag;
            _outFile = outFile;
        }

        public void Info(string format, params object[] args)
        {
            WriteWithLevel("INFO", format, args);
        }

        public void Warn(string format, params object[] args)
        {
            WriteWithLevel("WARN", format, args);
        }

        public void Error(Exception exception)
        {
            WriteWithLevel("ERROR", "Error : {0}\n", new object[]{exception.Message}, exception.StackTrace);
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            WriteWithLevel("ERROR", format, args, exception.StackTrace);
        }

        private void WriteWithLevel(string level, string format, object[] args, string optional = null)
        {
            var now = DateTime.Now;
            var prefix = now.ToShortDateString() + " - " + now.ToShortTimeString() + " ["+level+"] " + _tag + " - ";
            var formated = string.Format(format, args);
            var output = prefix + formated + "\n";

            if (!string.IsNullOrWhiteSpace(optional)) 
                output += optional;

            lock (SyncFile)
            {
                File.AppendAllText(_outFile.FullName, output, Encoding.UTF8);
                
            }
        }
    }
}