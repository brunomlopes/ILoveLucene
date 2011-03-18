using System.Collections.Generic;
using Core.Abstractions;
using Lucene.Net.Documents;

namespace Core.Extensions
{
    public class DocumentBuilder
    {
        private List<Field> _fields;

        public DocumentBuilder()
        {
            _fields = new List<Field>();
        }

        public DocumentBuilder Store(string name, string value)
        {
            this._fields.Add(new Field(name, value, Field.Store.YES, Field.Index.NO, Field.TermVector.NO));
            return this;
        }
        
        public DocumentBuilder Store(object name, string value)
        {
            this._fields.Add(new Field(name.ToString(), value, Field.Store.YES, Field.Index.NO, Field.TermVector.NO));
            return this;
        }

        public static implicit operator Document(DocumentBuilder builder)
        {
            var document = new Document();
            foreach (var field in builder._fields)
            {
                document.Add(field);
            }
            return document;
        }
    }

    public static class DocumentExtensions
    {
        public static DocumentBuilder Document(this IConverter self)
        {
            return new DocumentBuilder();
        }
        public static string String(this Document self, string field)
        {
            return self.GetField(field).StringValue();
        }
        
        public static int Int(this Document self, string field)
        {
            return int.Parse(self.GetField(field).StringValue());
        }

    }
}