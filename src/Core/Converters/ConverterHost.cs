using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Core.Abstractions;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace Core.Converters
{
    public class ConverterHost
    {
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
            var id = converter.ToId(item);
            writer.DeleteDocuments(new Term("_id", id));


            var nmspace = converter.GetNamespaceForItems();
            var name = converter.ToName(item);
            var document = converter.ToDocument(item);
            document.Add(new Field("_id", id, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field("_name", name, Field.Store.YES,
                                   Field.Index.ANALYZED,
                                   Field.TermVector.WITH_POSITIONS_OFFSETS));
            document.Add(new Field("_namespace", nmspace, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            writer.AddDocument(document);
        }

        public ICommand GetCommandForDocument(Document document)
        {
            var nmspace = document.GetField("_namespace").StringValue();
            if (!_convertersForNamespaces.ContainsKey(nmspace))
            {
                throw new NotImplementedException(string.Format("No converter for namespace {0} found", nmspace));
            }
            return _convertersForNamespaces[nmspace].FromDocumentToCommand(document);
        }
    }
}