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
        private readonly bool _writeDebug;

        public FileLogger(FileInfo outFile, string tag = null, bool writeDebug = false)
        {
            if (tag == null) tag = string.Empty;
            _tag = tag;
            _writeDebug = writeDebug;
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

        public void Debug(string format, params object[] args)
        {
            if(_writeDebug)
            {
                WriteWithLevel("DEBUG", format, args);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(format, args);
            }
        }

        private void WriteWithLevel(string level, string format, object[] args, string optional = null)
        {
            var now = DateTime.Now;
            var prefix = string.Format("{0} - {1}.{2:000} [{3}] {4} - ", now.ToShortDateString(), now.ToShortTimeString(), now.Millisecond, level, _tag);
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