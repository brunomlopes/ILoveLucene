using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ServiceProcess;
using System.Threading.Tasks;
using Core.Abstractions;
using System.Linq;
using ElevationHelper.Services;
using ElevationHelper.Services.WindowsServices;

namespace Plugins.Services
{
    [Export(typeof (IItemSource))]
    public class ServicesSource : BaseItemSource
    {
        public static readonly ElevatedChannel<IServiceHandler> ElevatedServiceHandler = new ElevatedChannel<IServiceHandler>();

        public override Task<IEnumerable<object>> GetItems()
        {
            return Task.Factory.StartNew(() => ServiceController.GetServices().Cast<object>());
        }

        public override bool Persistent
        {
            get { return false; }
        }
    }
}