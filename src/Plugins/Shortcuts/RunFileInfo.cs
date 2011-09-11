using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Core.API;
using Core.Abstractions;

namespace Plugins.Shortcuts
{
    [Export(typeof(IActOnItem))]
    public class Run : BaseActOnTypedItem<FileInfo>
    {
        public override void ActOn(FileInfo item)
        {
            Process.Start(item.FullName);
        }
    }
}