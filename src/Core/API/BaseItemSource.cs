using System.Collections.Generic;

namespace Core.API
{
    public abstract class BaseItemSource : IItemSource
    {
        public abstract IEnumerable<object> GetItems();

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