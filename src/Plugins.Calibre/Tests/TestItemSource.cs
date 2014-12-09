using System;
using Core.Abstractions;
using Xunit;
using System.Linq;

namespace Plugins.Calibre
{
    public class TestItemSource
    {
        [Fact]
        public void CanReturnBooksInMyPc()
        {
            var source = new CalibreBookSource();
            source.Log = new MockLog();

            source.Config = new Configuration();

            var items = source.GetItems().ToList();
            Assert.NotEqual(0, items.Count);
        }
        
    }

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

    internal class MockLog : ILog
    {
        public void Info(string format, params object[] args)
        {
            Debug(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Debug(format, args);
        }

        public void Error(Exception exception)
        {
            throw exception;
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            throw exception;
        }

        public void Debug(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(format, args);
        }
    }
}