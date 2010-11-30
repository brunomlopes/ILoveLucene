using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Threading.Tasks;
using Core.Abstractions;
using System.Linq;
using Core.Extensions;

namespace Core.Lucene
{
    // TODO: remove this if no longer needed for indexer
    public class TaskExecuter : IPartImportsSatisfiedNotification
    {
        private readonly CompositionContainer _mefContainer;

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<IBackgroundStartTask> Tasks { get; set; }

        public TaskExecuter(CompositionContainer mefContainer)
        {
            _mefContainer = mefContainer;
        }
        public void Start()
        {
            _mefContainer.SatisfyImportsOnce(this);
        }

        public void OnImportsSatisfied()
        {
            var tasks = Tasks;
            foreach (var task in tasks.Where(t => !t.Executed))
            {
                var t = task;
                Task.Factory.StartNew(t.Execute)
                    .GuardForException(e => Debug.WriteLine("Error running task '{0}':'{1}'", t, e));
            }
        }
    }
}