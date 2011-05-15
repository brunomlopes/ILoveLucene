using System;
using Core.Extensions;

namespace Core.Abstractions
{
    public interface IActOnItem
    {
        string Text { get; }
        Type TypedItemType { get; }
    }

    public static class NoReturnValue
    {
        private class NullTypedItem : IItem
        {
            public string Text
            {
                get { return null; }
            }

            public string Description
            {
                get { return null; }
            }

            public object Item
            {
                get { return null; }
            }
        }
        public static readonly IItem Object = new NullTypedItem();
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
    
    public interface IActOnTypedItemAndReturnTypedItem<in T, out TItem> : IActOnItem
    {
        ITypedItem<TItem> ActOn(T item);
    }
    
    public interface IActOnTypedItemAndReturnItem<in T> : IActOnItem
    {
        IItem ActOn(T item);
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

    public abstract class BaseActOnTypedItemAndReturnTypedItem<T, TReturnItem> : IActOnTypedItemAndReturnTypedItem<T, TReturnItem> 
    {
        public abstract ITypedItem<TReturnItem> ActOn(T item);

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
    
    public interface IActOnTypedItemWithArgumentsAndReturnTypedItem<in T, out TReturnItem> : IActOnItemWithArguments
    {
        /// <summary>
        /// Acts on the item with the given arguments
        /// </summary>
        /// <param name="arguments">Can be an empty string</param>
        ITypedItem<TReturnItem> ActOn(T item, string arguments);
    }

    public interface IActOnTypedItemWithAutoCompletedArguments<in T> : IActOnItemWithAutoCompletedArguments
    {
        /// <param name="arguments">Can be empty</param>
        ArgumentAutoCompletionResult AutoCompleteArguments(T item, string arguments);
    }
}