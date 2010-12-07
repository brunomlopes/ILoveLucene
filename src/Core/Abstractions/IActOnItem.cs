using System;

namespace Core.Abstractions
{
    public interface IActOnItem
    {
        string Text { get; }
        Type TypedItemType { get; }
    }

    /// <summary>
    /// Represents an action that is invalid
    /// </summary>
    public class InvalidActionException : ApplicationException
    {
        public InvalidActionException(string message) : base(message)
        {
        }
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
        void ActOn(T item);
    }

    public interface ICanActOnTypedItem<in T> : ICanActOnItem
    {
        bool CanActOn(T item);
    }
    
    public abstract class BaseActOnTypedItem<T> : IActOnTypedItem<T>
    {
        public abstract void ActOn(T item);

        public virtual string Text
        {
            get
            {
                var name = this.GetType().Name;
                return System.Text.RegularExpressions.Regex.Replace(name, "(?<l>[A-Z])", " ${l}").Trim();
            }
        }

        public Type TypedItemType
        {
            get { return this.GetTypedItemType(); }
        }
    }
    public abstract class BaseCommand<T> : BaseActOnTypedItem<T>,ITypedItem<T>
    {
        public abstract string Description { get; }
        public abstract T TypedItem { get; }
        public object Item { get { return TypedItem; } }
    }

    public interface IActOnTypedItemWithArguments<in T> : IActOnItemWithArguments
    {
        /// <summary>
        /// Acts on the item with the given arguments
        /// </summary>
        /// <param name="arguments">Can be an empty string</param>
        void ActOn(T item, string arguments);
    }

    public interface IActOnTypedItemWithAutoCompletedArguments<in T> : IActOnItemWithAutoCompletedArguments
    {
        /// <param name="arguments">Can be empty</param>
        ArgumentAutoCompletionResult AutoCompleteArguments(T item, string arguments);
    }
}