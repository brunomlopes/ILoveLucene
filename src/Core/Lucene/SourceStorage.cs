using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.API;
using Core.Abstractions;
using Lucene.Net.Analysis;
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
        private object _sourceWriteLock = new object();

        public SourceStorage(IItemSource source, Directory indexDirectory, LuceneStorage storage)
        {
            _source = source;
            _indexDirectory = indexDirectory;
            _storage = storage;
            
            EnsureIndexExistsAndThereIsNoWriteLock();
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

        private void EnsureIndexExistsAndThereIsNoWriteLock()
        {
            var createDir = true;
            var dir = _indexDirectory as FSDirectory;
            if (dir != null)
            {
                createDir = !dir.GetDirectory().Exists;
            }
            
            _indexDirectory.ClearLock("write.lock");
            new IndexWriter(_indexDirectory, new SimpleAnalyzer(),
                            createDir,
                            IndexWriter.MaxFieldLength.UNLIMITED).Close();
        }

        private void IndexItems(IItemSource source, IEnumerable<object> items)
        {
            lock (_sourceWriteLock)
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
        }

        public IndexWriter GetIndexWriter()
        {
            return new IndexWriter(_indexDirectory, new StandardAnalyzer(Version.LUCENE_29),
                                   IndexWriter.MaxFieldLength.UNLIMITED);
        }

        public void LearnCommandForInput(DocumentId completionId, string input)
        {
            var indexWriter = GetIndexWriter();
            try
            {
                _storage.LearnCommandForInput(indexWriter, completionId, input);
            }
            finally
            {
                indexWriter.Close();
            }
        }
    }
}