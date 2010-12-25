using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.Abstractions;
using Lucene.Net.Documents;

namespace Plugins.Commands
{
    [Export(typeof (IConverter))]
    public class ICommandConverter : IConverter<ICommand>
    {
        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<ICommand> Commands { get; set; }

        public IItem FromDocumentToItem(Document document)
        {
            var fullname = document.GetField("fullname").StringValue();
            var export =
                Commands.SingleOrDefault(c => c.GetType().FullName == fullname);

            if (export == null)
                throw new InvalidOperationException(string.Format("Missing Command {0}", fullname));
            return new CommandItem(export);
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