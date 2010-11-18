using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Version = Lucene.Net.Util.Version;

namespace Core.Converters
{
    public class ConverterHost
    {
        private class CommandId
        {
            public string Namespace { get; private set; }
            public string Id { get; private set; }

            public CommandId(Document document)
            {
                Namespace = document.GetField("_namespace").StringValue();
                Id = document.GetField("_id").StringValue();
            }
        }
        private Dictionary<string, IConverter> _convertersForNamespaces;

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

            var oldDocument = PopDocument(writer, id, nspace);

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


            var document = PopDocument(writer, id, nspace);

            if (document == null)
            {
                throw new InvalidOperationException(string.Format("Didn't find command for id '{0}' and namespace '{1}'", id,nspace));
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

        private Document PopDocument(IndexWriter writer, string id, string nspace)
        {
            var indexReader = writer.GetReader();
            var searcher = new IndexSearcher(indexReader);

            var parser = new QueryParser(Version.LUCENE_29, "_none", new StandardAnalyzer(Version.LUCENE_29));

            var query = parser.Parse(string.Format("(_id:\"{0}\") AND (_namespace:\"{1}\")", id, nspace));
            var documents = searcher.Search(query, 1);

            Debug.Assert(documents.totalHits <= 1, "Search for id and namespace should result in only one hit or none at all");

            if (documents.totalHits == 0) return null;

            var docNum = documents.scoreDocs.First().doc;
            var document = searcher.Doc(docNum);
            indexReader.DeleteDocument(docNum);
            return document;
        }
    }

}