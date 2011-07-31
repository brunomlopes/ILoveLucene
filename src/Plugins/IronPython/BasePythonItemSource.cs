using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Abstractions;

namespace Plugins.IronPython
{
    public abstract class BasePythonItemSource : BaseItemSource
    {
        public override Task<IEnumerable<object>> GetItems()
        {
            return Task.Factory.StartNew(() => GetAllItems());
        }

        protected abstract IEnumerable<object> GetAllItems();
    }
}