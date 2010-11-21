using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Version = Lucene.Net.Util.Version;

namespace Core.Lucene
{
    [Export(typeof(IBackgroundStartTask))]
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
            var createIndex = !Directory.GetDirectory().Exists;
            new IndexWriter(Directory, new StandardAnalyzer(Version.LUCENE_29), createIndex,
                            IndexWriter.MaxFieldLength.UNLIMITED).Close();
        }

        private void IndexItems(IItemSource source, IEnumerable<object> items, LuceneStorage host)
        {
            var indexWriter = GetIndexWriter();
            try
            {
                host.ClearSource(indexWriter, source);
                foreach (var item in items)
                {
                    host.UpdateDocumentForObject(indexWriter, source, item);
                }
                indexWriter.Commit();
            }
            finally
            {
                indexWriter.Close();
            }
        }
    }
}