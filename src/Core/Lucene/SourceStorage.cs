using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Core.Lucene
{
    public class SourceStorage
    {
        private readonly IItemSource _source;
        private readonly Directory _indexDirectory;
        private readonly LuceneStorage _storage;

        public SourceStorage(IItemSource source, Directory indexDirectory, LuceneStorage storage)
        {
            _source = source;
            _indexDirectory = indexDirectory;
            _storage = storage;

            EnsureIndexExists();
        }

        public IItemSource Source
        {
            // TODO: remove this 
            get {
                return _source;
            }
        }

        public Task IndexItems()
        {
            return _source.GetItems()
                .ContinueWith(task => IndexItems(_source, task.Result));
        }

        private void EnsureIndexExists()
        {
            var dir = _indexDirectory as FSDirectory;
            if (dir != null)
                new IndexWriter(dir, new StandardAnalyzer(Version.LUCENE_29), !dir.GetDirectory().Exists,
                                IndexWriter.MaxFieldLength.UNLIMITED).Close();
        }

        private void IndexItems(IItemSource source, IEnumerable<object> items)
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

        public void LearnCommandForInput(DocumentId completionId, string input)
        {
            _storage.LearnCommandForInput(GetIndexWriter(), completionId, input);
        }
    }
}