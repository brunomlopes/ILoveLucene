namespace Core.API
{
    public interface IActOnTypedItemAndReturnItem<in T> : IActOnItem
    {
        IItem ActOn(T item);
    }
}