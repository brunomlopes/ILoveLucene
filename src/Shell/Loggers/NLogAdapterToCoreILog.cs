using System;
using NLog;
using ILog = Core.Abstractions.ILog;

namespace ILoveLucene.Loggers
{
    public class NLogAdapterToCoreILog : ILog, Caliburn.Micro.ILog
    {
        private readonly Logger _logger;

        public NLogAdapterToCoreILog(Logger logger)
        {
            _logger = logger;
        }

        public void Info(string format, params object[] args)
        {
            _logger.Info(format,args);
        }

        public void Warn(string format, params object[] args)
        {
            _logger.Warn(format, args);
        }

        public void Error(Exception exception)
        {
            _logger.ErrorException(exception.Message, exception);
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            _logger.ErrorException(string.Format(format, args), exception);
        }

        public void Debug(string format, params object[] args)
        {
            _logger.Debug(format, args);
        }
    }
}