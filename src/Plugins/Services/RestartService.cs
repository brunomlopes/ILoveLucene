using System.ComponentModel.Composition;
using System.ServiceProcess;
using Core.Abstractions;

namespace Plugins.Services
{
    [Export(typeof (IActOnItem))]
    public class RestartService : BaseActOnTypedItem<ServiceController>, ICanActOnTypedItem<ServiceController>
    {
        public bool CanActOn(ServiceController item)
        {
            return item.Status == ServiceControllerStatus.Running;
        }

        public override void ActOn(ServiceController item)
        {
            ServicesSource.GetElevatedHandler().RestartService(item.ServiceName);
        }
    }
}