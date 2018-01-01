using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ILoveLucene.Infrastructure
{
    public class CoreLoggingJobHistoryPlugin : ISchedulerPlugin, IJobListener
    {
        private string _jobFailedMessage = "Job {1}.{0} execution failed and reports: {8}";
        private string _jobSuccessMessage = "Job {1}.{0} execution complete and reports: {8}";
        private string _jobToBeFiredMessage = "Job {1}.{0} fired (by trigger {4}.{3}) ";
        private string _jobWasVetoedMessage = "Job {1}.{0} was vetoed.  It was to be fired (by trigger {4}.{3})";
        private Core.Abstractions.ILog _log;
        private string _name;


        public virtual Task Initialize(string pluginName, IScheduler sched, CancellationToken token)
        {
            this._name = pluginName;
            sched.ListenerManager.AddJobListener(this, new IMatcher<JobKey>[] { EverythingMatcher<JobKey>.AllJobs() });
            return Task.CompletedTask;
        }

        public virtual Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken token)
        {
            var trigger = context.Trigger;
            var args = new object[] { context.JobDetail.Key.Name, context.JobDetail.Key.Group, SystemTime.UtcNow(), trigger.Key.Name, trigger.Key.Group, trigger.GetPreviousFireTimeUtc(), trigger.GetNextFireTimeUtc(), context.RefireCount };
            Log.Info(string.Format(CultureInfo.InvariantCulture, this.JobWasVetoedMessage, args));
            return Task.CompletedTask;
        }

        public virtual Task JobToBeExecuted(IJobExecutionContext context, CancellationToken token)
        {
                var trigger = context.Trigger;
                var args = new object[] { context.JobDetail.Key.Name, context.JobDetail.Key.Group, SystemTime.UtcNow(), trigger.Key.Name, trigger.Key.Group, trigger.GetPreviousFireTimeUtc(), trigger.GetNextFireTimeUtc(), context.RefireCount };
                Log.Info(string.Format(CultureInfo.InvariantCulture, this.JobToBeFiredMessage, args));
                return Task.CompletedTask;
        }

        public virtual Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken token)
        {
            object[] args;
            var trigger = context.Trigger;
            if (jobException != null)
            {
                string errMsg = jobException.Message;
                args = new object[] { context.JobDetail.Key.Name, context.JobDetail.Key.Group, SystemTime.UtcNow(), trigger.Key.Name, trigger.Key.Group, trigger.GetPreviousFireTimeUtc(), trigger.GetNextFireTimeUtc(), context.RefireCount, errMsg };
                this.Log.Warn(string.Format(CultureInfo.InvariantCulture, this.JobFailedMessage, args), jobException);
            }
            string result = Convert.ToString(context.Result, CultureInfo.InvariantCulture);
            args = new object[] { context.JobDetail.Key.Name, context.JobDetail.Key.Group, SystemTime.UtcNow(), trigger.Key.Name, trigger.Key.Group, trigger.GetPreviousFireTimeUtc(), trigger.GetNextFireTimeUtc(), context.RefireCount, result };
            Log.Info(string.Format(CultureInfo.InvariantCulture, this.JobSuccessMessage, args));
            return Task.CompletedTask;
        }

        public virtual Task Shutdown(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public virtual Task Start(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public virtual string JobFailedMessage
        {
            get
            {
                return this._jobFailedMessage;
            }
            set
            {
                this._jobFailedMessage = value;
            }
        }

        public virtual string JobSuccessMessage
        {
            get
            {
                return this._jobSuccessMessage;
            }
            set
            {
                this._jobSuccessMessage = value;
            }
        }

        public virtual string JobToBeFiredMessage
        {
            get
            {
                return this._jobToBeFiredMessage;
            }
            set
            {
                this._jobToBeFiredMessage = value;
            }
        }

        public virtual string JobWasVetoedMessage
        {
            get
            {
                return this._jobWasVetoedMessage;
            }
            set
            {
                this._jobWasVetoedMessage = value;
            }
        }

        public  global::Core.Abstractions.ILog Log
        {
            get
            {
                return this._log;
            }
            set
            {
                this._log = value;
            }
        }

        public virtual string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }
    }
}
