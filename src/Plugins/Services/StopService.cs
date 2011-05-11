using System.ComponentModel.Composition;
using System.ServiceProcess;
using Core.Abstractions;

namespace Plugins.Services
{
    [Export(typeof (IActOnItem))]
    public class StopService : BaseActOnTypedItem<ServiceController>, ICanActOnTypedItem<ServiceController>
    {
        public bool CanActOn(ServiceController item)
        {
            return item.Status == ServiceControllerStatus.Running;
        }

        public override void ActOn(ServiceController item)
        {
            ServicesSource.ElevatedServiceHandler.GetElevatedHandler().StopService(item.ServiceName);
        }
    }
}