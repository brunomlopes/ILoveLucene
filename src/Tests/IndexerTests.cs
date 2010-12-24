using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Lucene;
using ILoveLucene;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Xunit;

namespace Tests
{
    public class IndexerTests
    {
        private DirectoryInfo _storageLocation;
        private LearningStorage _learningStorage;

        public IndexerTests()
        {
            _storageLocation = new DirectoryInfo("learning");
            if (_storageLocation.Exists)
            {
                _storageLocation.Delete(true);
                _storageLocation.Refresh();
            }

            _learningStorage = new LearningStorage(_storageLocation);
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
            var firewall = new Item {Id = "Firewall"};
            var firefox = new Item {Id = "Firefox"};
            var directory = IndexItemIntoDirectory(firefox, firewall);

            var searcher = GetAutocompleter(directory, firefox, firewall);

            var results = searcher.Autocomplete("Fire");
            Assert.Equal("Firefox", results.AutoCompletedCommand.Item.Text);

            searcher.LearnInputForCommandResult("fire", results.OtherOptions.First());

            results = searcher.Autocomplete("Fire");

            Assert.True(results.HasAutoCompletion);
            Assert.Equal("Firewall", results.AutoCompletedCommand.Item.Text);
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
            var storage = new LuceneStorage(_learningStorage){Converters = new[] { new Converter() }};            
            var directory = new RAMDirectory();

            var source = new Source();
            var indexer = new SourceStorage(source, directory, storage);

            source.Items = new[] {new Item {Id = "simple"}};
            indexer.IndexItems().Wait();
            source.Items = new Item[] {};
            indexer.IndexItems().Wait();

            var searcher = GetAutocompleter(directory, storage);

            var results = searcher.Autocomplete("simple");
            Assert.False(results.HasAutoCompletion);
            Assert.Null(results.AutoCompletedCommand);
        }

        [Fact]
        public void CannotFindItemWhenIndexIsEmpty()
        {
            var directory = new RAMDirectory();
            var storage = new LuceneStorage(_learningStorage);

            var source = new Source();
            var indexer = new SourceStorage(source, directory, storage);

            source.Items = new Item[] {};
            indexer.IndexItems().Wait();

            var searcher = GetAutocompleter(directory, storage);

            var results = searcher.Autocomplete("simple");
            Assert.False(results.HasAutoCompletion);
            Assert.Null(results.AutoCompletedCommand);
        }

        private AutoCompleteBasedOnLucene GetAutocompleter(RAMDirectory directory)
        {
            return GetAutocompleter(directory, new LuceneStorage(_learningStorage) { Converters = new[] { new Converter() } });
        }

        private AutoCompleteBasedOnLucene GetAutocompleter(RAMDirectory directory, params Item[] items)
        {
            return GetAutocompleter(directory, new LuceneStorage(_learningStorage) { Converters = new[] { new Converter() } }, items);
        }

        private static AutoCompleteBasedOnLucene GetAutocompleter(RAMDirectory directory, LuceneStorage storage, IEnumerable<Item> items = null)
        {
            if (items == null) items = new Item[] {};

            var directoryFactory = new StaticDirectoryFactory(directory);
            var sourceStorage = new SourceStorageFactory(storage, directoryFactory)
                                    {Sources = new[] {new Source() {Items = items}}};
            var searcher = new AutoCompleteBasedOnLucene(directoryFactory, storage, sourceStorage, new DebugLogger());
            searcher.Configuration = new AutoCompleteConfiguration();
            searcher.Converters = new[] {new Converter()};
            return searcher;
        }

        private RAMDirectory IndexItemIntoDirectory(params Item[] items)
        {
            var storage = new LuceneStorage(_learningStorage) { Converters = new[] { new Converter() } };

            var directory = new RAMDirectory();
            var source = new Source();

            source.Items = items;
            var indexer = new SourceStorage(source, directory, storage);
            indexer.IndexItems().Wait();
            return directory;
        }
    }

    class Item
    {
        public string Id;
    }

    class SubItem : Item
    {
    }

    class Source : BaseItemSource
    {
        public IEnumerable<Item> Items { get; set; }

        public override Task<IEnumerable<object>> GetItems()
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