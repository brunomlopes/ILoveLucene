using Caliburn.Micro;
using NLog;
using NLog.Targets;

namespace ILoveLucene.Loggers
{
    [Target("BindableCollectionMemory")]
    public class BindableCollectionMemoryTarget : Target
    {
        private readonly int _limit;
        private object _messagesLock = new object();

        public LogLevel MinimumLogLevel { get; set; }

        public BindableCollectionMemoryTarget()
        {
            _messages = new BindableCollection<LogEventInfo>();
            _limit = 100;
            MinimumLogLevel = LogLevel.Info;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if(MinimumLogLevel > logEvent.Level) return;

            lock (_messagesLock)
            {
                Messages.Add(logEvent);
                if (Messages.Count > _limit)
                {
                    while (Messages.Count > _limit) Messages.RemoveAt(0);
                }
            }

        }

        private readonly IObservableCollection<LogEventInfo> _messages;

        public IObservableCollection<LogEventInfo> Messages
        {
            get { return _messages; }
        }
    }
}