using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Core.API;
using Core.Abstractions;

namespace Plugins.WindowsEnvironment
{
    [Export(typeof(IActOnItem))]
    public class OpenInNotepad2 : BaseActOnTypedItem<FileInfo>
    {
        public override void ActOn(FileInfo item)
        {
            Process.Start("notepad2", item.FullName);
        }
    }
}