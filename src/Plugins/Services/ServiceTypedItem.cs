using System.ServiceProcess;
using Core.Abstractions;

namespace Plugins.Services
{
    internal class ServiceTypedItem : ITypedItem<ServiceController>
    {
        private readonly ServiceController _serviceController;

        public ServiceTypedItem(ServiceController serviceController)
        {
            _serviceController = serviceController;
        }

        public string Text
        {
            get { return _serviceController.DisplayName; }
        }

        public string Description
        {
            get { return _serviceController.DisplayName + " - " + _serviceController.Status.ToString("g"); }
        }

        public object Item
        {
            get { return TypedItem; }
        }

        public ServiceController TypedItem
        {
            get { return _serviceController; }
        }
    }
}