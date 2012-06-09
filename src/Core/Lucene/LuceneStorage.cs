using System;
using System.Diagnostics;
using System.Linq;
using Core.API;
using Core.Abstractions;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Core.Lucene
{
    public class LuceneStorage
    {
        private readonly ILearningRepository _learningRepository;
        private readonly IConverterRepository _converterRepository;

        public LuceneStorage(ILearningRepository learningRepository, IConverterRepository converterRepository)
        {
            _learningRepository = learningRepository;
            _converterRepository = converterRepository;
        }

        public void UpdateDocumentForObject(IndexWriter writer, IndexReader reader, IItemSource source, string tag, object item)
        {
            var document = _converterRepository.ToDocument(source, item);

            var id = document.GetDocumentId();
            var documentId = id.GetId();
            var learningId = id.GetLearningId();

            PopDocument(writer, reader, documentId); //deleting the old version of the doc

            document.SetLearnings(_learningRepository.LearningsFor(learningId));
            document.Tag(tag);

            writer.AddDocument(document);
        }
   
        public AutoCompletionResult.CommandResult GetCommandResultForDocument(Document document, Explanation explanation)
        {
            var coreDoc = CoreDocument.Rehydrate(document);
            var command = _converterRepository.FromDocumentToItem(coreDoc);

            return new AutoCompletionResult.CommandResult(command, coreDoc.GetDocumentId(), explanation);
        }

        public void LearnCommandForInput(IndexWriter writer, IndexReader reader, DocumentId completionId, string input)
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


        public void DeleteDocumentsForSourceWithoutTag(IndexWriter indexWriter, IItemSource source, string tag)
        {
            var query = new BooleanQuery();
            query.Add(new BooleanClause(new TermQuery(new Term(SpecialFields.SourceId, source.Id)),
                                        BooleanClause.Occur.MUST));
            query.Add(new BooleanClause(new TermQuery(new Term(SpecialFields.Tag, tag)),
                                        BooleanClause.Occur.MUST_NOT));
            indexWriter.DeleteDocuments(query);
        }

        private Document PopDocument(IndexWriter writer, IndexReader reader, string sha1)
        {
            var searcher = new IndexSearcher(reader);
            try
            {
                var query = new TermQuery(new Term(SpecialFields.Sha1, sha1));
                var documents = searcher.Search(query, 1);

                Debug.Assert(documents.totalHits <= 1, string.Format("Sha1 '{0}' matched more than one document", sha1));

                if (documents.totalHits == 0) return null;

                var document = searcher.Doc(documents.scoreDocs.First().doc);
                writer.DeleteDocuments(new Term(SpecialFields.Sha1, sha1));
                return document;
            }
            finally
            {
                searcher.Close();
            }
        }
    }
}