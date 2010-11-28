using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Core.Abstractions;
using Core.Lucene;
using Plugins.Commands;
using Xunit;

namespace Tests
{
    [Export(typeof (IItem))]
    internal class SimpleCommand : IItem
    {
        public string Text
        {
            get { return "Text"; }
        }

        public string Description
        {
            get { return Description; }
        }

        public bool HasExecuted;

        public void Execute()
        {
            HasExecuted = true;
        }
    }

    public class ConverterHostTests
    {
        private CompositionContainer _container;
        private LuceneStorage _luceneStorage;
        private SimpleCommand _item;

        [Fact]
        public void CanConvertAnImplementationOfICommand()
        {
            ICommandConverter[] converters = SetupCommandConverter();

            Assert.Equal(converters.First(), _luceneStorage.GetConverter<SimpleCommand>());
            Assert.Equal(converters.First(), _luceneStorage.GetConverter<IItem>());
        }

        [Fact]
        public void ICommandConverterCanConvertCommandBackAndForth()
        {
            ICommandConverter[] converters = SetupCommandConverter();

            var converter = converters.First();
            var document = converter.ToDocument(_item);

            Assert.Equal(_item, converter.FromDocumentToCommand(document));
        }

        private ICommandConverter[] SetupCommandConverter()
        {
            _container = new CompositionContainer();
            var batch = new CompositionBatch();
            _item = new SimpleCommand();
            batch.AddExportedValue<IItem>(_item);

            _container.Compose(batch);

            var converters = new[] {new ICommandConverter(_container)};
            _luceneStorage = new LuceneStorage(converters);
            return converters;
        }
    }
}