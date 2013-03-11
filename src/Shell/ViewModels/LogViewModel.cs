using System.Collections.Generic;
using Caliburn.Micro;
using ILoveLucene.Loggers;
using NLog;

namespace ILoveLucene.ViewModels
{
    public class LogViewModel : PropertyChangedBase
    {
        private BindableCollectionMemoryTarget _inMemoryTarget;

        public BindableCollectionMemoryTarget InMemoryTarget
        {
            get { return _inMemoryTarget; }
            set
            {
                _inMemoryTarget = value;
                NotifyOfPropertyChange(() => InMemoryTarget);
                NotifyOfPropertyChange(() => Logs);
            }
        }

        public LogLevel MinimumLevel
        {
            get { return _inMemoryTarget.MinimumLogLevel; }
            set
            {
                if (Equals(value, _inMemoryTarget.MinimumLogLevel)) return;
                _inMemoryTarget.MinimumLogLevel = value;
                NotifyOfPropertyChange(() => MinimumLevel);
            }
        }

        public IEnumerable<LogLevel> LogLevels {get
        {
            return new LogLevel[]
                {
                    NLog.LogLevel.Debug,
                    NLog.LogLevel.Error,
                    NLog.LogLevel.Fatal,
                    NLog.LogLevel.Info,
                    NLog.LogLevel.Off,
                    NLog.LogLevel.Trace,
                    NLog.LogLevel.Warn,
                };
        }}

        public IObservableCollection<LogEventInfo> Logs
        {
            get { return InMemoryTarget.Messages; }
        }

        public LogViewModel(BindableCollectionMemoryTarget inMemoryTarget)
        {
            _inMemoryTarget = inMemoryTarget;
        }
    }
}