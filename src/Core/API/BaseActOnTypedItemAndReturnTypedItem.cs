using System;
using Core.Extensions;

namespace Core.API
{
    public abstract class BaseActOnTypedItemAndReturnTypedItem<T, TReturnItem> : IActOnTypedItemAndReturnTypedItem<T, TReturnItem> 
    {
        public abstract ITypedItem<TReturnItem> ActOn(T item);

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