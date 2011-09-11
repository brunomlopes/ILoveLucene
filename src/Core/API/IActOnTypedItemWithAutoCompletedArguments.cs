using Core.Abstractions;

namespace Core.API
{
    public interface IActOnTypedItemWithAutoCompletedArguments<in T> : IActOnItemWithAutoCompletedArguments
    {
        /// <param name="arguments">Can be empty</param>
        ArgumentAutoCompletionResult AutoCompleteArguments(T item, string arguments);
    }
}