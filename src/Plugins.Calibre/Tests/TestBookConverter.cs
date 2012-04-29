using Xunit;

namespace Plugins.Calibre.Tests
{
    public class TestBookConverter
    {
        [Fact]
        public void CanConvertABookBackAndForth()
        {

            var book = new Book()
                           {
                               Authors = "Authors",
                               Id = 42,
                               Title = "Title"
                           };

            book.Formats.Add("This is a format");
            book.Formats.Add("another.format");

            var bookConverter = new BookConverter();
            var coreDoc = bookConverter.ToDocument(new CalibreBookSource(), book);

            var convertedBook = (Book)bookConverter.FromDocumentToItem(coreDoc);
            Assert.Equal(book.Authors, convertedBook.Authors);
            Assert.Equal(book.Title, convertedBook.Title);
            Assert.Equal(book.Id, convertedBook.Id);
            Assert.Equal(book.Formats, convertedBook.Formats);
        }

    }
}