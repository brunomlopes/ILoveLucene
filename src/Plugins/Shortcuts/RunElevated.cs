using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Core.Abstractions;

namespace Plugins.Shortcuts
{
    [Export(typeof(IActOnItem))]
    public class RunElevated : BaseActOnTypedItem<FileInfo>
    {
        public override void ActOn(FileInfo item)
        {
            var arguments = new ProcessStartInfo(item.FullName);
            arguments.Verb = "runas";
            Process.Start(arguments);
        }
    }
}