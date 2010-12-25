using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Core.Abstractions;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Core.Lucene
{
    public interface IConverterRepository
    {
        IConverter<T> GetConverterForType<T>();
        IConverter GetConverterForId(string id);
    }

    public class ConverterRepository : IConverterRepository
    {
        private Dictionary<string, IConverter> _convertersPerId;

        private IEnumerable<IConverter> _converters;

        [ImportMany(typeof(IConverter))]
        public IEnumerable<IConverter> Converters
        {
            get { return _converters; }
            set
            {
                _converters = value;
                _convertersPerId = _converters.ToDictionary(c => c.GetId());
            }
        }

        public ConverterRepository()
        {
        }

        public ConverterRepository(params IConverter[] converters)
        {
            Converters = converters;
        }

        public IConverter<T> GetConverterForType<T>()
        {
            var converter = _convertersPerId.Select(kvp => kvp.Value).OfType<IConverter<T>>().FirstOrDefault();
            if (converter == null)
            {
                throw new NotImplementedException(string.Format("No converter for {0} found ", typeof(T)));
            }
            return converter;
        }

        public IConverter GetConverterForId(string id)
        {
            if (!_convertersPerId.ContainsKey(id))
            {
                throw new NotImplementedException(string.Format("No converter for id {0} found", id));
            }
            return _convertersPerId[id];
        }
    }

    public class LuceneStorage
    {
        private readonly ILearningRepository _learningRepository;
        private readonly IConverterRepository _converterRepository;


        public LuceneStorage(ILearningRepository learningRepository, IConverterRepository converterRepository)
        {
            _learningRepository = learningRepository;
            _converterRepository = converterRepository;
        }

        public void UpdateDocumentForObject(IndexWriter writer, IItemSource source, string tag, object item)
        {
            var type = item.GetType();
            GetType().GetMethod("UpdateDocumentForItem")
                .MakeGenericMethod(type)
                .Invoke(this, new[] {writer, source, tag, item});
        }

        public void UpdateDocumentForItem<T>(IndexWriter writer, IItemSource source, string tag, T item)
        {
            var converter = _converterRepository.GetConverterForType<T>();
            var converterId = converter.GetId();
            var id = converter.ToId(item);
            var name = converter.ToName(item);
            var document = converter.ToDocument(item);

            var sourceId = source.Id;

            var documentId = new DocumentId(converterId, id, sourceId);
            var hash = documentId.GetSha1();

            PopDocument(writer, hash); //deleting the old version of the doc

            var learnings = _learningRepository.LearningsFor(hash);

            document.Add(new Field(SpecialFields.Id, id, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field(SpecialFields.Name, name, Field.Store.YES,
                                   Field.Index.ANALYZED,
                                   Field.TermVector.WITH_POSITIONS_OFFSETS));
            document.Add(new Field(SpecialFields.Learnings, learnings, Field.Store.YES,
                                   Field.Index.ANALYZED,
                                   Field.TermVector.WITH_POSITIONS_OFFSETS));
            document.Add(new Field(SpecialFields.ConverterId, converterId, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field(SpecialFields.SourceId, sourceId, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field(SpecialFields.Tag, tag, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field(SpecialFields.Sha1, hash, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            writer.AddDocument(document);
        }

        public AutoCompletionResult.CommandResult GetCommandResultForDocument(Document document)
        {
            var converterId = document.GetField(SpecialFields.ConverterId).StringValue();
           
            var command = _converterRepository.GetConverterForId(converterId).FromDocumentToItem(document);

            return new AutoCompletionResult.CommandResult(command, new DocumentId(document));
        }

        public void LearnCommandForInput(IndexWriter writer, DocumentId commandId, string input)
        {
            // fickle command, isn't learnable
            if (commandId == null) return;

            string commandIdHash = commandId.GetSha1();
            var document = PopDocument(writer, commandIdHash);

            if (document == null)
                throw new InvalidOperationException(string.Format("Didn't find command {0}", commandId));

            var learnings = _learningRepository.LearnFor(input, commandIdHash);

            var field = document.GetField(SpecialFields.Learnings);
            if (field != null)
            {
                document.RemoveField(SpecialFields.Learnings);
            }
            var newField = new Field(SpecialFields.Learnings, learnings, Field.Store.YES, Field.Index.ANALYZED);
                
            document.Add(newField);

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

        

        private Document PopDocument(IndexWriter writer, string sha1)
        {
            var searcher = new IndexSearcher(writer.GetDirectory(), false);
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