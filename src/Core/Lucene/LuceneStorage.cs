using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Core.Abstractions;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Enumerable = System.Linq.Enumerable;

namespace Core.Lucene
{
    public class LuceneStorage
    {
        private class CommandId
        {
            public string Namespace { get; private set; }
            public string Id { get; private set; }

            public CommandId(string ns, string id)
            {
                Namespace = ns;
                Id = id;
            }

            public CommandId(Document document)
            {
                Namespace = document.GetField("_namespace").StringValue();
                Id = document.GetField("_id").StringValue();
            }

            public string GetSha1()
            {
                var sha1 = System.Security.Cryptography.SHA1.Create();
                sha1.Initialize();

                return
                    BitConverter.ToString(
                        sha1.ComputeHash(Enumerable.ToArray<byte>(Encoding.UTF8.GetBytes(Namespace).Concat(Encoding.UTF8.GetBytes(Id)))))
                        .Replace("-", "");
            }

            public override string ToString()
            {
                return string.Format("<Command Namespace:'{0}' Id:'{1}'>", Namespace, Id);
            }
        }
        private readonly Dictionary<string, IConverter> _convertersForNamespaces;

        public LuceneStorage(IEnumerable<IConverter> converters)
        {
            _convertersForNamespaces = converters.ToDictionary(c => IConverterExtensions.GetNamespaceForItems(c));
        }

        public IConverter<T> GetConverter<T>()
        {
            foreach (var converter in _convertersForNamespaces.Select(kvp => kvp.Value))
            {
                if (converter is IConverter<T>)
                    return (IConverter<T>)converter;
            }
            throw new NotImplementedException(string.Format("No converter for {0} found ", typeof(T)));
        }

        public void UpdateDocumentForObject(IndexWriter writer, object item)
        {
            var type = item.GetType();
            this.GetType().GetMethod("UpdateDocumentForItem").MakeGenericMethod(type)
                .Invoke(this, new[] {writer, item});
        }

        public void UpdateDocumentForItem<T>(IndexWriter writer, T item)
        {
            var converter = GetConverter<T>();
            var nspace = converter.GetNamespaceForItems();
            var id = converter.ToId(item);

            var sha1 = new CommandId(nspace, id).GetSha1();

            var oldDocument = PopDocument(writer, sha1);
            var learnings = string.Empty;
            if (oldDocument != null)
            {
                learnings = (oldDocument.GetField(SpecialFields.Learnings) ??
                             new Field(SpecialFields.Learnings, string.Empty, Field.Store.YES, Field.Index.ANALYZED)).
                    StringValue();
            }

            var name = converter.ToName(item);
            var document = converter.ToDocument(item);
            
            document.Add(new Field(SpecialFields.Id, id, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field(SpecialFields.Name, name, Field.Store.YES,
                                   Field.Index.ANALYZED,
                                   Field.TermVector.WITH_POSITIONS_OFFSETS));
            document.Add(new Field(SpecialFields.Learnings, learnings, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED,
                                   Field.TermVector.WITH_POSITIONS_OFFSETS));
            document.Add(new Field(SpecialFields.Namespace, nspace, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field(SpecialFields.Sha1, sha1, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            writer.AddDocument(document);
        }

        public AutoCompletionResult.CommandResult GetCommandResultForDocument(Document document)
        {
            var nspace = document.GetField(SpecialFields.Namespace).StringValue();
            if (!_convertersForNamespaces.ContainsKey(nspace))
            {
                throw new NotImplementedException(string.Format("No converter for namespace {0} found", nspace));
            }
            var command = _convertersForNamespaces[nspace].FromDocumentToCommand(document);

            return new AutoCompletionResult.CommandResult(command, new CommandId(document));
        }

        public void LearnCommandForInput(IndexWriter writer, object commandIdObject, string input)
        {
            // fickle command, isn't learnable
            if (commandIdObject == null) return;

            if(!(commandIdObject is CommandId))
                throw new InvalidOperationException("Id is not CommandId. It means the command didn't originate from this class");

            var commandId = (CommandId) commandIdObject;
            var document = PopDocument(writer, commandId.GetSha1());

            if (document == null)
                throw new InvalidOperationException(string.Format("Didn't find command {0}", commandId));

            var field = document.GetField(SpecialFields.Learnings);
            var learnings = input;
            if(field != null)
            {
                learnings = field.StringValue() + " " + learnings;
                document.RemoveField(SpecialFields.Learnings);
            }
            learnings = string.Join(" ", new HashSet<string>(writer.GetAnalyzer().Tokenize(learnings)));
            var newField = new Field(SpecialFields.Learnings, learnings, Field.Store.YES, Field.Index.NOT_ANALYZED);

            document.Add(newField);

            writer.AddDocument(document);
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