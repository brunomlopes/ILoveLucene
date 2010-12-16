using System;
using System.Collections.Generic;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Core.Lucene
{
    public class Indexer
    {
        private readonly Directory _indexDirectory;
        private readonly LuceneStorage _storage;

        public Indexer(Directory indexDirectory, LuceneStorage storage)
        {
            _indexDirectory = indexDirectory;
            _storage = storage;

            EnsureIndexExists();
        }

        private void EnsureIndexExists()
        {
            var dir = _indexDirectory as FSDirectory;
            if (dir != null)
                new IndexWriter(dir, new StandardAnalyzer(Version.LUCENE_29), !dir.GetDirectory().Exists,
                                IndexWriter.MaxFieldLength.UNLIMITED).Close();
        }

        public void IndexItems(IItemSource source, IEnumerable<object> items)
        {
            IndexWriter indexWriter = null;
            try
            {
                indexWriter = GetIndexWriter();
                var newTag = Guid.NewGuid().ToString();

                foreach (var item in items)
                {
                    _storage.UpdateDocumentForObject(indexWriter, source, newTag, item);
                }

                _storage.DeleteDocumentsForSourceWithoutTag(indexWriter, source, newTag);

                indexWriter.Commit();
            }
            finally
            {
                if (indexWriter != null) indexWriter.Close();
            }
        }

        public IndexWriter GetIndexWriter()
        {
            return new IndexWriter(_indexDirectory, new StandardAnalyzer(Version.LUCENE_29),
                                   IndexWriter.MaxFieldLength.UNLIMITED);
        }
    }
}