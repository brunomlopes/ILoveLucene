using System.ComponentModel.Composition;
using Core.Abstractions;
using Lucene.Net.Documents;

namespace Plugins.Calibre
{
    [Export(typeof(IConverter))]
    public class BookConverter : IConverter<Book>
    {
        public IItem FromDocumentToItem(Document document)
        {
            var book = new Book();
            book.Title = document.GetField("title").StringValue();
            book.Authors = document.GetField("authors").StringValue();
            book.Id = int.Parse(document.GetField("id").StringValue());
            foreach (var field in document.GetFields("format"))
            {
                book.Formats.Add(field.StringValue());
            }
            return book;
        }


        public string ToId(Book t)
        {
            return t.Id.ToString();
        }

        public Document ToDocument(Book t)
        {
            var document = new Document();
            document.Add(new Field("title", t.Title, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
            document.Add(new Field("authors", t.Authors, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
            document.Add(new Field("id", t.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));

            foreach (var format in t.Formats)
            {
                document.Add(new Field("format", format, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));                
            }
            return document;
        }

        public string ToName(Book t)
        {
            return t.Text;
        }

        public string ToType(Book t)
        {
            return "book";
        }
    }
}