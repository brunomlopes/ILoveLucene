namespace Core.API
{
    public interface ITypedItem<out T> : IItem
    {
        T TypedItem { get; }
    }
}