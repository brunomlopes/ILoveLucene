using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.API
{
    public abstract class BaseItemSource : IItemSource
    {
        public abstract IEnumerable<object> GetItems();

        public virtual string Id => GetType().FullName;

        public virtual bool Persistent => true;
    }
}