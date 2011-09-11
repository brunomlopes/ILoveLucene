using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Core.API;

namespace Plugins.Processes
{
    [Export(typeof (IConverter))]
    public class ProcessConverter : IConverter<Process>
    {
        public IItem FromDocumentToItem(CoreDocument document)
        {
            var id = int.Parse(document.GetString("id"));
            return new ProcessItem(Process.GetProcessById(id));
        }

        public string ToId(Process t)
        {
            return t.Id.ToString();
        }

        public CoreDocument ToDocument(IItemSource itemSource, Process t)
        {
            var document = new CoreDocument(itemSource, this, ToId(t), ToName(t), ToType(t))
                .Store("id", t.Id.ToString())
                .SetItemIdForLearning(ToName(t));
            return document;
        }

        public string ToName(Process t)
        {
            return t.ProcessName + " - " + t.MainWindowTitle;
        }

        public string ToType(Process t)
        {
            return t.MainWindowHandle != IntPtr.Zero ? "process window" : "process";
        }
    }
}