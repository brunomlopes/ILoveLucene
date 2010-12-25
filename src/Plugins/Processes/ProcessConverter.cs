using System.ComponentModel.Composition;
using System.Diagnostics;
using Core.Abstractions;
using Lucene.Net.Documents;

namespace Plugins.Processes
{
    [Export(typeof(IConverter))]
    public class ProcessConverter : IConverter<Process>
    {
        public IItem FromDocumentToItem(Document document)
        {
            var id = int.Parse(document.GetField("id").StringValue());
            return new ProcessItem(Process.GetProcessById(id));
        }

        public string ToId(Process t)
        {
            return t.Id.ToString();
        }

        public Document ToDocument(Process t)
        {
            var document = new Document();
            document.Add(new Field("id", t.Id.ToString(), Field.Store.YES, Field.Index.NO));
            return document;
        }

        public string ToName(Process t)
        {
            return t.ProcessName;
        }
    }
}