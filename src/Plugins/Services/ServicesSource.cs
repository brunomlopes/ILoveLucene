using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ServiceProcess;
using System.Threading.Tasks;
using Core.Abstractions;
using System.Linq;

namespace Plugins.Services
{
    // TODO: The actions require elevation...
    [Export(typeof (IItemSource))]
    public class ServicesSource : BaseItemSource
    {
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