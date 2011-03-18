using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Core.Abstractions;
using Lucene.Net.Documents;
using Core.Extensions;

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
            return this.Document().Store("id", t.Id.ToString());
        }

        public string ToName(Process t)
        {
            return t.ProcessName + " - " + t.MainWindowTitle;
        }

        public string ToType(Process t)
        {
            return t.GetType().Name;
        }
    }
}