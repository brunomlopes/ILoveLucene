namespace Core.API
{
    public interface IActOnTypedItemWithArguments<in T> : IActOnItemWithArguments
    {
        /// <summary>
        /// Acts on the item with the given arguments
        /// </summary>
        /// <param name="arguments">Can be an empty string</param>
        void ActOn(T item, string arguments);
    }
}