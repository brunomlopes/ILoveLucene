using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ServiceProcess;
using Core.API;
using ElevationHelper.Services;
using ElevationHelper.Services.WindowsServices;

namespace Plugins.Services
{
    [Export(typeof (IItemSource))]
    public class ServicesSource : BaseItemSource
    {
        public static readonly ElevatedChannel<IServiceHandler> ElevatedServiceHandler = new ElevatedChannel<IServiceHandler>();

        public override IEnumerable<object> GetItems()
        {
            return ServiceController.GetServices();
        }

        public override bool Persistent
        {
            get { return false; }
        }
    }
}