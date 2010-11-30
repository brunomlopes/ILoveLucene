using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Core.Lucene
{
    [Export(typeof(IIndexer))]
    public class Indexer : LuceneBase, IBackgroundStartTask, IIndexer
    {
        [ImportMany]
        public IEnumerable<IConverter> Converters { get; set; }

        [ImportMany]
        public IEnumerable<IItemSource> Sources { get; set; }

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

        public bool Executed { get; private set; }

        public void Execute()
        {
            Index();
        }

        public void Index()
        {
            var host = new LuceneStorage(Converters);

            Sources
                .AsParallel()
                .Where(s => s.NeedsReindexing)
                .ForAll(s => s.GetItems()
                                 .ContinueWith(task =>
                                                   {
                                                       var items = task.Result;

                                                       IndexItems(s, items, host);
                                                   }));

            Executed = true;
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
                var conf = host.GetObject<Configuration>(indexWriter, "IndexerConfiguration");
                if (conf == null) conf = new Configuration();
                var newTag = conf.SetNewTagForSourceId(host.SourceId(source));

                foreach (var item in items)
                {
                    host.UpdateDocumentForObject(indexWriter, source, newTag, item);
                }

                host.DeleteDocumentsForSourceWithoutTag(indexWriter, source, newTag);

                host.StoreObject(indexWriter, "IndexerConfiguration", conf);
                indexWriter.Commit();
            }
            finally
            {
                if (indexWriter != null) indexWriter.Close();
            }
        }

        class Configuration
        {
            public Dictionary<string, Guid> Tags = new Dictionary<string, Guid>();

            public string SetNewTagForSourceId(string sourceId)
            {
                var newGuid = Guid.NewGuid();
                Debug.WriteLine(string.Format("New tag '{0}' for source id '{1}'", newGuid, sourceId));
                return (Tags[sourceId] = newGuid).ToString();
            }
        }
    }
}