using System.ComponentModel.Composition;
using System.Diagnostics;
using Core.Abstractions;

namespace Plugins.Processes
{
    [Export(typeof(IActOnItem))]
    public class KillProcess : BaseActOnTypedItem<Process>
    {
        public override void ActOn(Process item)
        {
            item.Kill();
        }
    }
}