namespace Core.API
{
    public interface IActOnTypedItem<in T> : IActOnItem
    {
        void ActOn(T item);
    }
}