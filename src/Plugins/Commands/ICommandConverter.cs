using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Core.Abstractions;
using Lucene.Net.Documents;

namespace Plugins.Commands
{
    [Export(typeof (IConverter))]
    public class ICommandConverter : IConverter<IItem>
    {
        private readonly CompositionContainer _mefContainer;

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<IItem> Commands { get; set; }

        [ImportingConstructor]
        public ICommandConverter(CompositionContainer mefContainer)
        {
            _mefContainer = mefContainer;
            _mefContainer.SatisfyImportsOnce(this);
        }

        public Type ConvertedType
        {
            get { return typeof (IItem); }
        }

        public IItem FromDocumentToCommand(Document document)
        {
            var fullname = document.GetField("fullname").StringValue();
            var export =
                Commands.SingleOrDefault(c => c.GetType().FullName == fullname);

            if (export == null)
                throw new InvalidOperationException(string.Format("Missing ICommand {0}", fullname));
            return export;
        }

        public string ToId(IItem t)
        {
            return t.GetType().FullName;
        }

        public Document ToDocument(IItem t)
        {
            var document = new Document();
            document.Add(new Field("fullname", t.GetType().FullName, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
            return document;
        }

        public string ToName(IItem t)
        {
            return t.Text;
        }
    }
}