using System;
using Core.Abstractions;
using NLog;

namespace ILoveLucene.Loggers
{
    public class NLogAdapter : ILog, Caliburn.Micro.ILog
    {
        private readonly Logger _logger;

        public NLogAdapter(Logger logger)
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