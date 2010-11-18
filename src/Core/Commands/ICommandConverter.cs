using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Core.Abstractions;
using Lucene.Net.Documents;

namespace Core.Commands
{
    [Export(typeof(IConverter))]
    public class ICommandConverter : IConverter<ICommand>
    {
        private readonly CompositionContainer _container;

        [ImportingConstructor]
        public ICommandConverter(CompositionContainer container)
        {
            _container = container;
        }

        public Type ConvertedType
        {
            get { return typeof (ICommand); }
        }

        public ICommand FromDocumentToCommand(Document document)
        {
            var fullname = document.GetField("fullname").StringValue();
            var export =
                _container.GetExportedValues<ICommand>(AttributedModelServices.GetContractName(typeof (ICommand)))
                    .SingleOrDefault(c => c.GetType().FullName == fullname);

            if(export == null)
                throw new InvalidOperationException(string.Format("Missing ICommand {0}", fullname));
            return export;
        }

        public string ToId(ICommand t)
        {
            return t.GetType().FullName;
        }

        public Document ToDocument(ICommand t)
        {
            var document = new Document();
            document.Add(new Field("fullname", t.GetType().FullName, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
            return document;
        }

        public string ToName(ICommand t)
        {
            return t.Text;
        }
    }
}