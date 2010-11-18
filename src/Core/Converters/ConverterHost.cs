using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Core.Abstractions;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Core.Converters
{
    public class ConverterHost
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
                        sha1.ComputeHash(Encoding.UTF8.GetBytes(Namespace).Concat(Encoding.UTF8.GetBytes(Id)).ToArray()))
                        .Replace("-", "");
            }

            public override string ToString()
            {
                return string.Format("<Command Namespace:'{0}' Id:'{1}'>", Namespace, Id);
            }
        }
        private readonly Dictionary<string, IConverter> _convertersForNamespaces;

        public ConverterHost(IEnumerable<IConverter> converters)
        {
            _convertersForNamespaces = converters.ToDictionary(c => c.GetNamespaceForItems());
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
            //writer.DeleteDocuments(new Term("_sha1", sha1));

            Document oldDocument = PopDocument(writer, sha1);
            string learnings = string.Empty;
            if(oldDocument != null){
                learnings = (oldDocument.GetField("_learnings") ??
                             new Field("_learnings", string.Empty, Field.Store.YES, Field.Index.ANALYZED)).StringValue();
            }

            var name = converter.ToName(item);
            var document = converter.ToDocument(item);
            
            document.Add(new Field("_id", id, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field("_name", name, Field.Store.YES,
                                   Field.Index.ANALYZED,
                                   Field.TermVector.WITH_POSITIONS_OFFSETS));
            document.Add(new Field("_learnings", learnings, Field.Store.YES,
                                   Field.Index.ANALYZED,
                                   Field.TermVector.WITH_POSITIONS_OFFSETS));
            document.Add(new Field("_namespace", nspace, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field("_sha1", sha1, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            writer.AddDocument(document);
        }

        public AutoCompletionResult.CommandResult GetCommandResultForDocument(Document document)
        {
            var nmspace = document.GetField("_namespace").StringValue();
            if (!_convertersForNamespaces.ContainsKey(nmspace))
            {
                throw new NotImplementedException(string.Format("No converter for namespace {0} found", nmspace));
            }
            var command = _convertersForNamespaces[nmspace].FromDocumentToCommand(document);
            
            return new AutoCompletionResult.CommandResult(command, new CommandId(document));
        }

        public void LearnCommandForInput(IndexWriter writer, object commandIdObject, string input)
        {
            if (commandIdObject == null)
            {
                // fickle command, isn't learnable
                return;
            }
            if(!(commandIdObject is CommandId))
            {
                throw new InvalidOperationException("Id is not CommandId. It means the command didn't originate from this class");
            }
            var commandId = (CommandId) commandIdObject;
            var id = commandId.Id;
            var nspace = commandId.Namespace;

            var document = PopDocument(writer, commandId.GetSha1());

            if (document == null)
            {
                throw new InvalidOperationException(string.Format("Didn't find command {0}", commandId));
            }
            var field = document.GetField("_learnings");
            string learnings = string.Empty;
            if(field != null)
            {
                learnings = field.StringValue() + " " + input;
                document.RemoveField("_learnings");
            }
            var newField = new Field("_learnings", learnings, Field.Store.YES, Field.Index.ANALYZED);

            document.Add(newField);

            writer.AddDocument(document);
        }

        private Document PopDocument(IndexWriter writer, string sha1)
        {
            var searcher = new IndexSearcher(writer.GetDirectory(), false);
            try
            {
                var query = new TermQuery(new Term("_sha1", sha1));
                var documents = searcher.Search(query, 1);

                Debug.Assert(documents.totalHits <= 1, string.Format("Sha1 {0} matched more than one document", sha1));

                if (documents.totalHits == 0) return null;

                var docNum = documents.scoreDocs.First().doc;
                var document = searcher.Doc(docNum);
                writer.DeleteDocuments(new Term("_sha1", sha1));
                return document;
            }
            finally
            {
                searcher.Close();
            }
            
        }
    }

}