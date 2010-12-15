using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
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
        private DirectoryInfo _storageLocation;

        public IndexerTests()
        {
            _storageLocation = new DirectoryInfo("learning");
            if (_storageLocation.Exists)
            {
                _storageLocation.Delete(true);
                _storageLocation.Refresh();
            }
        }

        [Fact]
        public void CanFindItemWhenItIsIndexed()
        {
            var item = new Item {Id = "simple"};
            var directory = IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter(directory);

            var results = searcher.Autocomplete("simple");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal(results.AutoCompletedCommand.Item.Text, "simple");
        }

        [Fact]
        public void CanFindItemWithBadSpelling()
        {
            var item = new Item {Id = "Firefox"};

            var directory = IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter(directory);

            var results = searcher.Autocomplete("Firafox");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal(results.AutoCompletedCommand.Item.Text, "Firefox");
        }
        
        [Fact]
        public void CanFindSubItemWithItemConverter()
        {
            var item = new SubItem {Id = "Firefox"};

            var directory = IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter(directory);

            var results = searcher.Autocomplete("FireFox");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal(results.AutoCompletedCommand.Item.Text, "Firefox");
        }
        
        [Fact]
        public void CanLearnItem()
        {
            var item = new Item {Id = "Firewall"};
            var directory = IndexItemIntoDirectory(new Item {Id = "Firefox"}, item);

            var searcher = GetAutocompleter(directory);

            var results = searcher.Autocomplete("Fire");
            searcher.LearnInputForCommandResult("fire", results.OtherOptions.First());

            results = searcher.Autocomplete("Fire");

            Assert.True(results.HasAutoCompletion);
            Assert.Equal(results.AutoCompletedCommand.Item.Text, "Firewall");
        }

        [Fact]
        public void CanFindItemBasedOnSubstring()
        {
            var item = new Item {Id = "EmacsClient.lnk"};
            var directory = IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter(directory);

            var results = searcher.Autocomplete("emac");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal(results.AutoCompletedCommand.Item.Text, "EmacsClient.lnk");
        } 
        
        [Fact]
        public void CanFindItemBasedOnSubstringOnTheEnd()
        {
            var item = new Item {Id = "SQLyog.lnk"};
            var directory = IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter(directory);

            var results = searcher.Autocomplete("yog");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal(results.AutoCompletedCommand.Item.Text, "SQLyog.lnk");
        }

        [Fact]
        public void CannotFindItemWhenItHasNothingToDoWithTheQuery()
        {
            var item = new Item {Id = "EmacsClient.lnk"};

            var directory = IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter(directory);

            var results = searcher.Autocomplete("Firefox");
            Assert.False(results.HasAutoCompletion);
        }

        [Fact]
        public void CannotFindItemWhenItIsRemovedAfterBeingIndexed()
        {
            
            var directory = new RAMDirectory();
            var indexer = new Indexer(directory, _storageLocation);
            indexer.Converters = new[] { new Converter() };

            var source = new Source();

            source.Items = new[] {new Item {Id = "simple"}};
            var luceneStorage = new LuceneStorage(indexer.Converters, _storageLocation);
            indexer.IndexItems(source, source.Items);
            source.Items = new Item[] {};
            indexer.IndexItems(source, source.Items);

            var searcher = GetAutocompleter(directory);

            var results = searcher.Autocomplete("simple");
            Assert.False(results.HasAutoCompletion);
            Assert.Null(results.AutoCompletedCommand);
        }

        [Fact]
        public void CannotFindItemWhenIndexIsEmpty()
        {
            var directory = new RAMDirectory();
            var indexer = new Indexer(directory, _storageLocation);
            indexer.Converters = new[] { new Converter() };

            var source = new Source();

            source.Items = new Item[] {};
            indexer.IndexItems(source, source.Items);

            var searcher = GetAutocompleter(directory);

            var results = searcher.Autocomplete("simple");
            Assert.False(results.HasAutoCompletion);
            Assert.Null(results.AutoCompletedCommand);
        }

        private AutoCompleteBasedOnLucene GetAutocompleter(RAMDirectory directory)
        {
            var searcher = AutoCompleteBasedOnLucene.WithDirectory(directory, _storageLocation);
            searcher.Configuration = new AutoCompleteConfiguration();
            searcher.Converters = new[] {new Converter()};
            return searcher;
        }

        private RAMDirectory IndexItemIntoDirectory(params Item[] items)
        {
            var directory = new RAMDirectory();
            var indexer = new Indexer(directory, _storageLocation);
            indexer.Converters = new[] { new Converter() };
            var source = new Source();

            source.Items = items;
            indexer.IndexItems(source, source.Items);
            return directory;
        }
    }

    class Item
    {
        public string Id;
    }

    class SubItem : Item
    {
        public string Name;
    }

    class Source : IItemSource
    {
        public IEnumerable<Item> Items { get; set; }

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