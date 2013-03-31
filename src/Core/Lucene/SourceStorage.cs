using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.API;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Core.Lucene
{
    public class SourceStorage
    {
        private readonly Directory _indexDirectory;
        private readonly ILearningRepository _learningRepository;
        private readonly IConverterRepository _converterRepository;
        private const string EmptyTag = "THIS_IS_AN_EMPTY_TAG";

        public SourceStorage(Directory indexDirectory,
                             ILearningRepository learningRepository,
                             IConverterRepository converterRepository)
        {
            _indexDirectory = indexDirectory;

            _learningRepository = learningRepository;
            _converterRepository = converterRepository;

            EnsureIndexExists();
        }

        public void IndexItems(IItemSource source, IEnumerable<object> items)
        {
            using (var indexWriter = GetIndexWriter())
            using (var indexReader = indexWriter.GetReader())
            {
                var newTag = Guid.NewGuid().ToString();

                foreach (var item in items)
                {
                    UpdateDocumentForObject(indexWriter, indexReader, source, newTag, item);
                }

                DeleteDocumentsForSourceWithoutTag(indexWriter, source, newTag);

                indexWriter.Commit();
            }
        }

        public void AppendItems(IItemSource source, params object[] items)
        {
            using (var indexWriter = GetIndexWriter())
            using (var indexReader = indexWriter.GetReader())
            {
                foreach (var item in items)
                {
                    UpdateDocumentForObject(indexWriter, indexReader, source, EmptyTag, item);
                }

                indexWriter.Commit();
            }
        }

        public void RemoveItems(IItemSource source, params object[] items)
        {
            using (var indexWriter = GetIndexWriter())
            using (var indexReader = indexWriter.GetReader())
            {
                foreach (var item in items)
                {
                    DeleteDocumentForObject(indexWriter, indexReader, source, item);
                }

                indexWriter.Commit();
            }
        }

        public void LearnCommandForInput(DocumentId completionId, string input)
        {
            using (var indexWriter = GetIndexWriter())
            using (var indexReader = indexWriter.GetReader())
            {
                LearnCommandForInput(indexWriter, indexReader, completionId, input);
            }
        }

        private void EnsureIndexExists()
        {
            var dir = _indexDirectory as FSDirectory;
            if (dir == null) return;

            if(IndexWriter.IsLocked(_indexDirectory))
                IndexWriter.Unlock(_indexDirectory);

            using (var writer = new IndexWriter(dir, new StandardAnalyzer(Version.LUCENE_29), !dir.Directory.Exists,
                                   IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // index exists, we're good.
            }
        }

        private void UpdateDocumentForObject(IndexWriter writer, IndexReader reader, IItemSource source, string tag, object item)
        {
            var document = _converterRepository.ToDocument(source, item);

            var id = document.GetDocumentId();
            var documentId = id.GetId();
            var learningId = id.GetLearningId();

            PopDocument(writer, reader, documentId); //deleting the old version of the doc

            document.SetLearnings(_learningRepository.LearningsFor(learningId));
            if (tag != null)
            {
                document.Tag(tag);
            }

            writer.AddDocument(document);
        }

        public void DeleteDocumentForObject(IndexWriter writer, IndexReader indexReader, IItemSource source, object item)
        {
            var document = _converterRepository.ToDocument(source, item);

            var id = document.GetDocumentId();
            var documentId = id.GetId();
            PopDocument(writer, indexReader, documentId);
        }


        private IndexWriter GetIndexWriter()
        {
            return new IndexWriter(_indexDirectory, new StandardAnalyzer(Version.LUCENE_29),
                                   IndexWriter.MaxFieldLength.UNLIMITED);
        }

        private void DeleteDocumentsForSourceWithoutTag(IndexWriter indexWriter, IItemSource source, string tag)
        {
            var query = new BooleanQuery();
            query.Add(new BooleanClause(new TermQuery(new Term(SpecialFields.SourceId, source.Id)),
                                        Occur.MUST));
            query.Add(new BooleanClause(new TermQuery(new Term(SpecialFields.Tag, EmptyTag)),
                                        Occur.MUST_NOT));
            query.Add(new BooleanClause(new TermQuery(new Term(SpecialFields.Tag, tag)),
                                        Occur.MUST_NOT));
            indexWriter.DeleteDocuments(query);
        }

        private void LearnCommandForInput(IndexWriter writer, IndexReader reader, DocumentId completionId, string input)
        {
            // fickle command, isn't learnable
            if (completionId == null) return;

            var document = CoreDocument.Rehydrate(PopDocument(writer, reader, completionId.GetId()));

            if (document == null)
                throw new InvalidOperationException(string.Format("Didn't find command {0}", completionId));

            var learnings = _learningRepository.LearnFor(input, completionId.GetLearningId());

            document.SetLearnings(learnings);

            writer.AddDocument(document);
        }

        private Document PopDocument(IndexWriter writer, IndexReader reader, string sha1)
        {
            using(var searcher = new IndexSearcher(reader))
            {
                var query = new TermQuery(new Term(SpecialFields.Sha1, sha1));
                var documents = searcher.Search(query, 1);

                Debug.Assert(documents.TotalHits <= 1, string.Format("Sha1 '{0}' matched more than one document", sha1));

                if (documents.TotalHits == 0) return null;

                var document = searcher.Doc(documents.ScoreDocs.First().Doc);
                writer.DeleteDocuments(new Term(SpecialFields.Sha1, sha1));
                return document;
            }
        }
    }
}