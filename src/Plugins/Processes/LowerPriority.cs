using System.ComponentModel.Composition;
using System.Diagnostics;
using Core.API;
using Core.Abstractions;

namespace Plugins.Processes
{
    [Export(typeof(IActOnItem))]
    public class LowerPriority : BaseActOnTypedItem<Process>
    {
        public override void ActOn(Process item)
        {
            item.PriorityClass = ProcessPriorityClass.BelowNormal;
        }
    }
}