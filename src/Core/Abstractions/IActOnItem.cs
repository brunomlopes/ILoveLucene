using System;

namespace Core.Abstractions
{
    public interface IActOnItem
    {
        string Text { get; }
        Type TypedItemType { get; }
    }
    
    public interface ICanActOnItem
    {
    }

    public interface IActOnItemWithArguments : IActOnItem
    {
    }
    
    public interface IActOnItemWithAutoCompletedArguments : IActOnItem
    {
    }

    public interface IActOnTypedItem<in T> : IActOnItem
    {
        void ActOn(ITypedItem<T> item);
    }

    public interface ICanActOnTypedItem<in T> : ICanActOnItem
    {
        bool CanActOn(ITypedItem<T> item);
    }
    
    public abstract class BaseActOnTypedItem<T> : IActOnTypedItem<T>
    {
        public abstract void ActOn(ITypedItem<T> item);

        public abstract string Text { get; }

        public Type TypedItemType
        {
            get { return this.GetTypedItemType(); }
        }
    }
    public abstract class BaseCommand<T> : BaseActOnTypedItem<T>,ITypedItem<T>
    {
        public abstract string Description { get; }
        public abstract T Item { get; }
    }

    public interface IActOnTypedItemWithArguments<in T> : IActOnItemWithArguments
    {
        /// <summary>
        /// Acts on the item with the given arguments
        /// </summary>
        /// <param name="arguments">Can be an empty string</param>
        void ActOn(ITypedItem<T> item, string arguments);
    }

    public interface IActOnTypedItemWithAutoCompletedArguments<in T> : IActOnItemWithAutoCompletedArguments
    {
        /// <param name="arguments">Can be empty</param>
        ArgumentAutoCompletionResult AutoCompleteArguments(ITypedItem<T> item, string arguments);
    }
}