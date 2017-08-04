using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Core.API;

namespace Plugins.Shortcuts
{
    [Export(typeof(IActOnItem))]
    public class RunPowershellScript : BaseActOnTypedItem<FileInfo>, ICanActOnTypedItem<FileInfo>
    {
        public override void ActOn(FileInfo item)
        {
            var arguments = new ProcessStartInfo("powershell.exe")
            {
                Arguments = $"-file {item.FullName}"
            };
            Process.Start(arguments);
        }


        public bool CanActOn(FileInfo item)
        {
            return item.FullName.EndsWith(".ps1", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}