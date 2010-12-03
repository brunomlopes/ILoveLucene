using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Quartz;
using Version = Lucene.Net.Util.Version;
using Core.Extensions;

namespace Core.Lucene
{
    [Export(typeof(IStartupTask))]
    public class Indexer : LuceneBase, IStartupTask, IStatefulJob
    {
        [Export("IIndexer.JobGroup")]
        public const string JobGroup = "Indexers";

        [ImportMany]
        public IEnumerable<IConverter> Converters { get; set; }

        [ImportMany]
        public IEnumerable<IItemSource> Sources { get; set; }

        [Import]
        public IScheduler Scheduler { get; set; }

        [Import]
        public IndexerConfiguration Configuration { get; set; }


        public Indexer()
        {
            EnsureIndexExists();
            Converters = new IConverter[] { };
            Sources = new IItemSource[] { };
        }
        
        public Indexer(Directory directory)
            :base(directory)
        {
            EnsureIndexExists();
            Converters = new IConverter[] {};
            Sources = new IItemSource[] {};
        }

        void IStartupTask.Execute()
        {
            foreach (var itemSource in Sources)
            {
                var frequency = Configuration.GetFrequencyForItemSource(itemSource);

                var jobDetail = new JobDetail("IndexerFor" + itemSource, JobGroup, typeof(Indexer));
                jobDetail.JobDataMap["source"] = itemSource;

                var trigger = TriggerUtils.MakeSecondlyTrigger(frequency);

                // add 2 seconds to "try" and ensure the first time gets executed always
                trigger.StartTimeUtc = TriggerUtils.GetEvenMinuteDate(DateTime.UtcNow.AddSeconds(2));
                trigger.Name = "Each"+frequency+"SecondsFor"+itemSource;
                trigger.MisfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithRemainingCount;

                Scheduler.ScheduleJob(jobDetail, trigger);
            }
        }

        void IJob.Execute(JobExecutionContext context)
        {
            var source = (IItemSource) context.MergedJobDataMap["source"];
            Debug.WriteLine("Indexing item source " + source);
            source.GetItems()
                .ContinueWith(task => IndexItems(source, task.Result, new LuceneStorage(Converters)))
                .GuardForException(e => Debug.WriteLine("Exception while indexing {0}:{1}", source, e));
        }

        private void EnsureIndexExists()
        {
            var dir = Directory as FSDirectory;
            if (dir != null)
                new IndexWriter(Directory, new StandardAnalyzer(Version.LUCENE_29), !dir.GetDirectory().Exists,
                                IndexWriter.MaxFieldLength.UNLIMITED).Close();
        }

        public void IndexItems(IItemSource source, IEnumerable<object> items, LuceneStorage host)
        {
            IndexWriter indexWriter = null;
            try
            {
                indexWriter = GetIndexWriter();
                var newTag = Guid.NewGuid().ToString();

                foreach (var item in items)
                {
                    host.UpdateDocumentForObject(indexWriter, source, newTag, item);
                }

                host.DeleteDocumentsForSourceWithoutTag(indexWriter, source, newTag);

                indexWriter.Commit();
            }
            finally
            {
                if (indexWriter != null) indexWriter.Close();
            }
        }
    }
}