using System;
using Core.Extensions;

namespace Core.API
{
    public abstract class BaseActOnTypedItem<T> : IActOnTypedItem<T>
    {
        public abstract void ActOn(T item);

        public virtual string Text
        {
            get { return this.FriendlyTypeName(); }
        }

        public Type TypedItemType
        {
            get { return this.GetTypedItemType(); }
        }
    }
}