using System.ComponentModel.Composition;
using Core.API;
using Core.Abstractions;
using Lucene.Net.Documents;

namespace Plugins.Calibre
{
    [Export(typeof(IConverter))]
    public class BookConverter : IConverter<Book>
    {
        public IItem FromDocumentToItem(CoreDocument document)
        {
            var book = new Book();
            book.Title = document.GetString("title");
            book.Authors = document.GetString("authors");
            book.Id = int.Parse(document.GetString("id"));
            book.Formats.AddRange(document.GetStringList("formats"));
            
            return book;
        }

        public string ToId(Book t)
        {
            return t.Id.ToString();
        }

        public CoreDocument ToDocument(IItemSource itemSource, Book t)
        {
            var coreDoc = new CoreDocument(itemSource, this, ToId(t), ToName(t), ToType(t));

            coreDoc.Store("title", t.Title)
                .Store("authors", t.Authors)
                .Store("id", t.Id.ToString())
                .Store("format", t.Formats.ToArray());

            return coreDoc;
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