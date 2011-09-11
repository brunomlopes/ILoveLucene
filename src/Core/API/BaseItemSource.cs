using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.API
{
    public abstract class BaseItemSource : IItemSource
    {
        public abstract Task<IEnumerable<object>> GetItems();

        public virtual string Id
        {
            get { return GetType().FullName; }
        }

        public virtual bool Persistent
        {
            get { return true; }
        }
    }
}