using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.API;
using Core.Abstractions;
using Core.Lucene;
using ILoveLucene.Loggers;
using Lucene.Net.Store;
using Xunit;
using Directory = Lucene.Net.Store.Directory;
using Shouldly;

namespace Tests
{
    public class IndexerTests
    {
        private readonly DirectoryInfo _storageLocation;
        private readonly FileSystemLearningRepository _learningRepository;
        private readonly ConverterRepository _converterRepository;
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

            var indexLocation = new DirectoryInfo("index");
            if(indexLocation.Exists)
            {
                indexLocation.Delete(true);
                indexLocation.Refresh();
            }

            _converterRepository= new ConverterRepository(new TextItemConverter());
            
            _directory = FSDirectory.Open(indexLocation);
        }

        [Fact]
        public void CanFindItemWhenItIsIndexed()
        {
            var item = new TextItem("simple");
            IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("simple");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal("simple", results.AutoCompletedCommand.Item.Text);
        }
        [Fact]
        public void WhenAnItemIsUpdated_ThereIsStillJustOneItemFound()
        {
            var item = new TextItem("simple");
            IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("simple");
            results.OtherOptions.Count().ShouldBe(0);
            results.HasAutoCompletion.ShouldBeTrue();
            results.AutoCompletedCommand.Item.Text.ShouldBe("simple");

            IndexItemIntoDirectory(item);

            searcher = GetAutocompleter();

            results = searcher.Autocomplete("simple");
            results.OtherOptions.Count().ShouldBe(0);
            results.HasAutoCompletion.ShouldBeTrue();
            results.AutoCompletedCommand.Item.Text.ShouldBe("simple");

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
            Assert.Equal("Firefox", results.AutoCompletedCommand.Item.Text);
        }
        
        [Fact]
        public void CanFindSubItemWithItemConverter()
        {
            var item = new SubItem ("Firefox");

            IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("FireFox");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal("Firefox", results.AutoCompletedCommand.Item.Text);
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
            Assert.Single(results.OtherOptions);
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
            Assert.Equal("EmacsClient.lnk", results.AutoCompletedCommand.Item.Text);
        }

        [Fact]
        public void CanFindItemBasedOnSubstringOnTheEnd()
        {
            var item = new TextItem ("SQLyog.lnk");
            IndexItemIntoDirectory(item);

            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("yog");
            Assert.True(results.HasAutoCompletion);
            Assert.Equal("SQLyog.lnk", results.AutoCompletedCommand.Item.Text);
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
            var source = new Source();
            var indexer = new SourceStorage(_directory, _learningRepository, _converterRepository);

            source.Items = new[] {new TextItem ("simple")};
            indexer.IndexItems(source, source.GetItems());
            source.Items = new TextItem[] {};
            indexer.IndexItems(source, source.GetItems());


            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("simple");
            Assert.False(results.HasAutoCompletion);
            Assert.Null(results.AutoCompletedCommand);
        }

        [Fact]
        public void CannotFindItemWhenIndexIsEmpty()
        {
            var source = new Source();
            var indexer = new SourceStorage(_directory, _learningRepository, _converterRepository);

            source.Items = new TextItem[] {};
            indexer.IndexItems(source, source.GetItems());

            var searcher = GetAutocompleter();

            var results = searcher.Autocomplete("simple");
            Assert.False(results.HasAutoCompletion);
            Assert.Null(results.AutoCompletedCommand);
        }

        [Fact]
        public void CanFindPreviousItemsEvenWhenInTheMiddleOfAnIndex()
        {
            {
                var source = new Source { Items = new  []{new TextItem("Firefox")} };
                new SourceStorage(_directory, _learningRepository, _converterRepository)
                    .IndexItems(source, source.GetItems());
            }
            {
                var source = new StepSource {Items = new[] {new TextItem("Firewall")}};

                var task = Task.Factory.StartNew(() =>
                    {
                        IEnumerable<object> enumerable = source.GetItems();
                        new SourceStorage(_directory, _learningRepository, _converterRepository)
                            .IndexItems(source, enumerable);
                    });

                var searcher = GetAutocompleter();
                var results = searcher.Autocomplete("Fire");
                Assert.True(results.HasAutoCompletion);
                Assert.Equal("Firefox",results.AutoCompletedCommand.Item.Text);

                Assert.True(source.ReleaseNextItemAndWait(TimeSpan.FromSeconds(5)));
                task.Wait(TimeSpan.FromSeconds(5));

                searcher = GetAutocompleter();
                results = searcher.Autocomplete("Fire");
                Assert.True(results.HasAutoCompletion);
                Assert.Equal("Firewall", results.AutoCompletedCommand.Item.Text);
            }
        }



        [Fact]
        public void DoesNotDeleteItemsWithNoTagsWhenIndexing()
        {
            {
                var source = new Source { Items = new TextItem[] { } };
                new SourceStorage(_directory, _learningRepository, _converterRepository).AppendItems(source, new TextItem("Firewall"));
            }
            {
                var source = new Source { Items = new[] { new TextItem("Firefox") } };
                new SourceStorage(_directory, _learningRepository, _converterRepository).IndexItems(source, source.GetItems());
            }
            {
                var searcher = GetAutocompleter();

                var results = searcher.Autocomplete("Firewall");
                Assert.True(results.HasAutoCompletion);
                Assert.Equal("Firewall", results.AutoCompletedCommand.Item.Text);
            }
        }


        [Fact]
        public void CanDeleteItemsFromSource()
        {
            {
                var source = new Source { Items = new TextItem[] { } };
                new SourceStorage(_directory, _learningRepository, _converterRepository).AppendItems(source, new TextItem("Firewall"));
            }
            {
                var searcher = GetAutocompleter();

                var results = searcher.Autocomplete("Firewall");
                Assert.True(results.HasAutoCompletion);
                Assert.Equal("Firewall", results.AutoCompletedCommand.Item.Text);
            }
            {
                var source = new Source { Items = new TextItem[] { } };
                new SourceStorage(_directory, _learningRepository, _converterRepository).RemoveItems(source, new TextItem("Firewall"));
            }
            {
                var searcher = GetAutocompleter();

                var results = searcher.Autocomplete("Firewall");
                Assert.False(results.HasAutoCompletion);
            }
        }

        private AutoCompleteBasedOnLucene GetAutocompleter(params IItem[] items)
        {
            if (items == null) items = new IItem[] {};

            var directoryFactory = new StaticDirectoryFactory(_directory);
            var source = new Source {Items = items};
            var sourceStorage = new SourceStorageFactory(directoryFactory, _converterRepository, _learningRepository)
                                    {Sources = new[] {source}};

            var searcher = new AutoCompleteBasedOnLucene(directoryFactory,  sourceStorage, new DebugLogger(), _converterRepository);
            searcher.Configuration = new AutoCompleteConfiguration();
            searcher.Converters = new[] {new TextItemConverter()};
            return searcher;
        }

        private void IndexItemIntoDirectory(params IItem[] items)
        {
            var source = new Source {Items = items};

            var indexer = new SourceStorage(_directory,_learningRepository, _converterRepository);
            indexer.IndexItems(source, source.GetItems());
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

        public override IEnumerable<object> GetItems()
        {
            return Items;
        }

        public override string Id
        {
            get { return "Tests.Source"; }
        }
    }

    class StepSource : BaseItemSource
    {
        public IEnumerable<IItem> Items { get; set; }
        private readonly SemaphoreSlim _itemsWaiter;
        private readonly SemaphoreSlim _releasedANewItem;


        public StepSource()
        {
            _itemsWaiter = new SemaphoreSlim(0);
            _releasedANewItem = new SemaphoreSlim(0);
        }

        public override IEnumerable<object> GetItems()
        {
            return StepIterateItems();
        }

        public bool ReleaseNextItemAndWait(TimeSpan waitTimeout)
        {
            _itemsWaiter.Release();
            return _releasedANewItem.Wait(waitTimeout);
        }

        public override string Id
        {
            get { return "Tests.Source"; } // Fake to be the same as the source
        }

        private IEnumerable<object> StepIterateItems()
        {
            foreach (var item in Items)
            {
                _itemsWaiter.Wait();
                yield return item;
                _releasedANewItem.Release();
            }
        } 
    }

    class TextItemConverter : IConverter<TextItem>
    {
        public IItem FromDocumentToItem(CoreDocument document)
        {
            return new TextItem(document.GetString("id"));
        }

        public string ToId(TextItem t)
        {
            return t.Text;
        }

        public CoreDocument ToDocument(IItemSource itemSource, TextItem t)
        {
            var document = new CoreDocument(itemSource, this, ToId(t), ToName(t), ToType(t));
            document.Store("id", t.Text);
            return document;
        }

        public string ToName(TextItem t)
        {
            return t.Text;
        }

        public string ToType(TextItem t)
        {
            return t.GetType().Name;
        }
    }
}