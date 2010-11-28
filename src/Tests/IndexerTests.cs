using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Lucene;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Xunit;

namespace Tests
{
    public class IndexerTests
    {
        [Fact]
        public void CanFindItemWhenItIsIndexed()
        {
            var directory = new RAMDirectory();
            var indexer = new Indexer(directory);
            indexer.Converters = new[] { new Converter() };

            var source = new Source();

            source.Items = new[] {new Item {Id = "simple"}};
            indexer.IndexItems(source, source.Items, new LuceneStorage(indexer.Converters));

            var searcher = AutoCompleteBasedOnLucene.WithDirectory(directory);
            searcher.Converters = new[] {new Converter()};

            var results = searcher.Autocomplete("simple");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal(results.AutoCompletedCommand.Item.Text, "simple");
        }
        
        [Fact]
        public void CannotFindItemWhenItIsRemovedAfterBeingIndexed()
        {
            var directory = new RAMDirectory();
            var indexer = new Indexer(directory);
            indexer.Converters = new[] { new Converter() };

            var source = new Source();

            source.Items = new[] {new Item {Id = "simple"}};
            indexer.IndexItems(source, source.Items, new LuceneStorage(indexer.Converters));
            source.Items = new Item[] {};
            indexer.IndexItems(source, source.Items, new LuceneStorage(indexer.Converters));

            var searcher = AutoCompleteBasedOnLucene.WithDirectory(directory);
            searcher.Converters = new[] {new Converter()};

            var results = searcher.Autocomplete("simple");
            Assert.False(results.HasAutoCompletion);
            Assert.Null(results.AutoCompletedCommand);
        }
        
        [Fact]
        public void CannotFindItemWhenIndexIsEmpty()
        {
            var directory = new RAMDirectory();
            var indexer = new Indexer(directory);
            indexer.Converters = new[] { new Converter() };

            var source = new Source();

            source.Items = new Item[] {};
            indexer.IndexItems(source, source.Items, new LuceneStorage(indexer.Converters));

            var searcher = AutoCompleteBasedOnLucene.WithDirectory(directory);
            searcher.Converters = new[] {new Converter()};

            var results = searcher.Autocomplete("simple");
            Assert.False(results.HasAutoCompletion);
            Assert.Null(results.AutoCompletedCommand);
        }
    }

    class Item
    {
        public string Id;
    }

    class Source : IItemSource
    {
        public IEnumerable<Item> Items { get; set; }
        public bool NeedsReindexing { get; set; }

        public Task<IEnumerable<object>> GetItems()
        {
            return Task.Factory.StartNew(() => Items.Cast<object>());
        }
    }

    class Converter : IConverter<Item>
    {
        public Type ConvertedType
        {
            get { return typeof (Item); }
        }

        public IItem FromDocumentToItem(Document document)
        {
            return new TextItem(document.GetField("id").StringValue());
        }

        public string ToId(Item t)
        {
            return t.Id;
        }

        public Document ToDocument(Item t)
        {
            var document = new Document();
            document.Add(new Field("id", t.Id, Field.Store.YES, Field.Index.NO));
            return document;
        }

        public string ToName(Item t)
        {
            return t.Id;
        }
    }
}