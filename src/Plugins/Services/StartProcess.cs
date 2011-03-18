using System.ComponentModel.Composition;
using System.ServiceProcess;
using Core.Abstractions;

namespace Plugins.Services
{
    [Export(typeof (IActOnItem))]
    public class StartProcess : BaseActOnTypedItem<ServiceController>, ICanActOnTypedItem<ServiceController>
    {
        public bool CanActOn(ServiceController item)
        {
            return item.Status == ServiceControllerStatus.Stopped || item.Status == ServiceControllerStatus.Paused;
        }

        public override void ActOn(ServiceController item)
        {
            if (item.Status == ServiceControllerStatus.Paused)
                item.Continue();
            else if (item.Status == ServiceControllerStatus.Stopped)
                item.Start();
        }
    }
}