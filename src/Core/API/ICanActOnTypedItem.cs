namespace Core.API
{
    public interface ICanActOnTypedItem<in T> : ICanActOnItem
    {
        bool CanActOn(T item);
    }
}