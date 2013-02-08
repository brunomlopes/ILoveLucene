using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

namespace Core.Scheduler
{
    public class MefJobFactory : IJobFactory
    {
        private readonly SimpleJobFactory _simpleFactory;
        private readonly CompositionContainer _container;

        public MefJobFactory(SimpleJobFactory simpleFactory, CompositionContainer container)
        {
            _simpleFactory = simpleFactory;
            _container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var job = _simpleFactory.NewJob(bundle, scheduler);
            _container.SatisfyImportsOnce(job);
            return job;
        }
    }
}