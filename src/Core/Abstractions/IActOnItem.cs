using System;
using Core.Extensions;

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
            get { return this.FriendlyTypeName(); }
        }

        public Type TypedItemType
        {
            get { return this.GetTypedItemType(); }
        }
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