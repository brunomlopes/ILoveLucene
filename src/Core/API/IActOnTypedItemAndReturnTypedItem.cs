namespace Core.API
{
    public interface IActOnTypedItemAndReturnTypedItem<in T, out TItem> : IActOnItem
    {
        ITypedItem<TItem> ActOn(T item);
    }
}