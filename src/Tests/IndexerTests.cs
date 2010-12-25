using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Lucene;
using ILoveLucene;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Xunit;
using Directory = Lucene.Net.Store.Directory;

namespace Tests
{
    public class IndexerTests
    {
        private readonly DirectoryInfo _storageLocation;
        private readonly FileSystemLearningRepository _learningRepository;
        private readonly Directory _directory;

        public IndexerTests()
        {
            _storageLocation = new DirectoryInfo("learning");
            if (_storageLocation.Exists)
            {
                _storageLocation.Delete(true);
                _storageLocation.Refresh();
            }

            _learningRepository = new FileSystemLearningRepository(_storageLocation);

            _directory = new RAMDirectory();
        }

        [Fact]
        public void CanFindItemWhenItIsIndexed()
        {
            var item = new TextItem("simple");
            IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("simple");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal(results.AutoCompletedCommand.Item.Text, "simple");
        }
        
        [Fact]
        public void ConvertsToCorrectItemType()
        {
            var item = new TextItem ("simple");
            IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("simple");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal(item.GetType(), results.AutoCompletedCommand.Item.GetType());
        }

        [Fact]
        public void CanFindItemWithBadSpelling()
        {
            var item = new TextItem("Firefox");

            IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("Firafox");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal(results.AutoCompletedCommand.Item.Text, "Firefox");
        }
        
        [Fact]
        public void CanFindSubItemWithItemConverter()
        {
            var item = new SubItem ("Firefox");

            IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("FireFox");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal(results.AutoCompletedCommand.Item.Text, "Firefox");
        }
        
        [Fact]
        public void CanLearnItem()
        {
            var firewall = new TextItem ("Firewall");
            var firefox = new TextItem ("Firefox");

            IndexItemIntoDirectory(firefox, firewall);

            var searcher = GetAutocompleter(firefox, firewall);

            var results = searcher.Autocomplete("Fire");
            Assert.Equal("Firefox", results.AutoCompletedCommand.Item.Text);

            searcher.LearnInputForCommandResult("fire", results.OtherOptions.First());

            results = searcher.Autocomplete("Fire");

            Assert.True(results.HasAutoCompletion);
            Assert.Equal(1, results.OtherOptions.Count());
            Assert.Equal("Firewall", results.AutoCompletedCommand.Item.Text);
        }

        [Fact]
        public void CanFindItemBasedOnSubstring()
        {
            var item = new TextItem ("EmacsClient.lnk");
            IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("emac");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal(results.AutoCompletedCommand.Item.Text, "EmacsClient.lnk");
        }

        [Fact]
        public void CanFindItemBasedOnSubstringOnTheEnd()
        {
            var item = new TextItem ("SQLyog.lnk");
            IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("yog");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal(results.AutoCompletedCommand.Item.Text, "SQLyog.lnk");
        }

        [Fact]
        public void CannotFindItemWhenItHasNothingToDoWithTheQuery()
        {
            var item = new TextItem ("EmacsClient.lnk");

            IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("Firefox");
            Assert.False(results.HasAutoCompletion);
        }

        [Fact]
        public void CannotFindItemWhenItIsRemovedAfterBeingIndexed()
        {
            var storage = new LuceneStorage(_learningRepository){Converters = new[] { new Converter() }};            

            var source = new Source();
            var indexer = new SourceStorage(source, _directory, storage);

            source.Items = new[] {new TextItem ("simple")};
            indexer.IndexItems().Wait();
            source.Items = new TextItem[] {};
            indexer.IndexItems().Wait();

            var searcher = GetAutocompleter(storage);

            var results = searcher.Autocomplete("simple");
            Assert.False(results.HasAutoCompletion);
            Assert.Null(results.AutoCompletedCommand);
        }

        [Fact]
        public void CannotFindItemWhenIndexIsEmpty()
        {
            var storage = new LuceneStorage(_learningRepository);

            var source = new Source();
            var indexer = new SourceStorage(source, _directory, storage);

            source.Items = new TextItem[] {};
            indexer.IndexItems().Wait();

            var searcher = GetAutocompleter(storage);

            var results = searcher.Autocomplete("simple");
            Assert.False(results.HasAutoCompletion);
            Assert.Null(results.AutoCompletedCommand);
        }

        private AutoCompleteBasedOnLucene GetAutocompleter()
        {
            return GetAutocompleter(new LuceneStorage(_learningRepository) { Converters = new[] { new Converter() } });
        }

        private AutoCompleteBasedOnLucene GetAutocompleter(params TextItem[] items)
        {
            return GetAutocompleter(new LuceneStorage(_learningRepository) { Converters = new[] { new Converter() } }, items);
        }

        private AutoCompleteBasedOnLucene GetAutocompleter(LuceneStorage storage, IEnumerable<IItem> items = null)
        {
            if (items == null) items = new TextItem[] {};

            var directoryFactory = new StaticDirectoryFactory(_directory);
            var source = new Source {Items = items};
            var sourceStorage = new SourceStorageFactory(storage, directoryFactory)
                                    {Sources = new[] {source}};

            var searcher = new AutoCompleteBasedOnLucene(directoryFactory, storage, sourceStorage, new DebugLogger());
            searcher.Configuration = new AutoCompleteConfiguration();
            searcher.Converters = new[] {new Converter()};
            return searcher;
        }

        private void IndexItemIntoDirectory(params IItem[] items)
        {
            var storage = new LuceneStorage(_learningRepository) { Converters = new[] { new Converter() } };
            var source = new Source {Items = items};

            var indexer = new SourceStorage(source, _directory, storage);
            indexer.IndexItems().Wait();
        }
    }


    class SubItem : TextItem
    {
        public SubItem(string input) : base(input)
        {
        }

        public SubItem(string input, string description) : base(input, description)
        {
        }
    }

    class Source : BaseItemSource
    {
        public IEnumerable<IItem> Items { get; set; }

        public override Task<IEnumerable<object>> GetItems()
        {
            return Task.Factory.StartNew(() => Items.Cast<object>());
        }
    }

    class Converter : IConverter<TextItem>
    {
        public Type ConvertedType
        {
            get { return typeof (TextItem); }
        }

        public IItem FromDocumentToItem(Document document)
        {
            return new TextItem(document.GetField("id").StringValue());
        }

        public string ToId(TextItem t)
        {
            return t.Text;
        }

        public Document ToDocument(TextItem t)
        {
            var document = new Document();
            document.Add(new Field("id", t.Text, Field.Store.YES, Field.Index.NO));
            return document;
        }

        public string ToName(TextItem t)
        {
            return t.Text;
        }
    }
}