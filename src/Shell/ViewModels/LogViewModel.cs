using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using ILoveLucene.Loggers;
using NLog;
using LogManager = NLog.LogManager;

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

        public IObservableCollection<LogEventInfo> Logs
        {
            get { return InMemoryTarget.Messages; }
        }

        public LogViewModel(BindableCollectionMemoryTarget inMemoryTarget)
        {
            _inMemoryTarget = inMemoryTarget;
            //_inMemoryTarget.CollectionChanged += (s, e) =>
            //                                         {
            //                                             NotifyOfPropertyChange(() => InMemoryTarget);
            //                                             NotifyOfPropertyChange(() => Logs);
            //                                         };
        }
    }
}