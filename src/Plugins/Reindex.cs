using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Core.Abstractions;
using Plugins.Commands;
using Quartz;
using System.Linq;

namespace Plugins
{
    // TODO: implement this with new commands
    //[Export(typeof(IItem))]
    //[Export(typeof(IActOnItem))]
    //public class Reindex : BaseCommand<Reindex>, IActOnTypedItemWithAutoCompletedArguments<Reindex>, IActOnTypedItemWithArguments<Reindex>
    //{
    //    [Import]
    //    public IScheduler Scheduler { get; set; }

    //    [Import("IIndexer.JobGroup")]
    //    public string JobGroup { get; set; }


    //    // FIXME: this looks fugly :(
    //    public override void Act()
    //    {
    //        throw new InvalidActionException("Need a index to be triggered");
    //    }

    //    public ArgumentAutoCompletionResult AutoCompleteArguments(Reindex item, string arguments)
    //    {
    //        arguments = arguments.ToLowerInvariant();
    //        return ArgumentAutoCompletionResult
    //            .OrderedResult(arguments,
    //                           Scheduler.GetJobNames(JobGroup)
    //                               .Where(j => j.ToLowerInvariant().Contains(arguments)));
    //    }

    //    public void ActOn(Reindex item, string arguments)
    //    {
    //        Scheduler.TriggerJobWithVolatileTrigger(arguments, JobGroup);
    //    }

    //    public override string Text
    //    {
    //        get { return "Reindex"; }
    //    }

    //    public override string Description
    //    {
    //        get { return "Triggers a reindex of a source"; }
    //    }

    //    public override Reindex TypedItem
    //    {
    //        get { return this; }
    //    }
    
    [Export(typeof(ICommand))]
    public class ReindexAll : BaseCommand
    {
        [Import]
        public IScheduler Scheduler { get; private set; }

        [Import("IIndexer.JobGroup")]
        public string JobGroup { get; set; }

        public override void Act()
        {
            foreach (var jobName in Scheduler.GetJobNames(JobGroup))
            {
                Scheduler.TriggerJobWithVolatileTrigger(jobName, JobGroup);
            }
        }
    }
}