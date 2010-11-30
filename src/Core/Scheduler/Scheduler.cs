using System;
using System.ComponentModel.Composition.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Simpl;

namespace Core.Scheduler
{
    public class Scheduler
    {
        private readonly CompositionContainer _container;
        private IScheduler _scheduler;

        public Scheduler(CompositionContainer container)
        {
            var schedulerFactory = new StdSchedulerFactory();

            _scheduler = schedulerFactory.GetScheduler();
            _container = container;
        }

        public void Start()
        {
            _scheduler.Start();

            var jobDetail = new JobDetail("indexer", null, typeof(IndexerJob));
            _scheduler.JobFactory = new MefJobFactory(new SimpleJobFactory(), _container);

            var trigger = TriggerUtils.MakeMinutelyTrigger(30);

            trigger.StartTimeUtc = TriggerUtils.GetEvenMinuteDate(DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
            trigger.Name = "Each30Minutes";
            trigger.MisfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithRemainingCount;

            _scheduler.ScheduleJob(jobDetail, trigger);
        }

        public void Shutdown()
        {
            _scheduler.Shutdown(false); // TODO: show we wait for the jobs to finish?
        }
    }
}