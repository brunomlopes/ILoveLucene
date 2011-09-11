namespace Core.API
{
    public interface IActOnTypedItemWithArgumentsAndReturnTypedItem<in T, out TReturnItem> : IActOnItemWithArguments
    {
        /// <summary>
        /// Acts on the item with the given arguments
        /// </summary>
        /// <param name="arguments">Can be an empty string</param>
        ITypedItem<TReturnItem> ActOn(T item, string arguments);
    }
}