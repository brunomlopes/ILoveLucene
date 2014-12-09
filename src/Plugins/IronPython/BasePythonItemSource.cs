using System.Collections.Generic;
using Core.API;

namespace Plugins.IronPython
{
    public abstract class BasePythonItemSource : BaseItemSource
    {
        public override IEnumerable<object> GetItems()
        {
            return GetAllItems();
        }

        protected abstract IEnumerable<object> GetAllItems();
    }
}