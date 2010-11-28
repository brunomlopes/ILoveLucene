using System;
using System.Reflection;

namespace Core.Abstractions
{
    public interface IActOnItem
    {
        string Text { get; }
        Type TypedItemType { get; }
    }

    public interface IActOnItemWithArguments : IActOnItem
    {
    }
    
    public interface IActOnItemWithAutoCompletedArguments : IActOnItem
    {
    }

    public static class IActOnItemExtension
    {
        public static void ActOn(this IActOnItem self, IItem item)
        {
            self.GetType()
                .GetMethod("ActOn", BindingFlags.Instance | BindingFlags.Public, null, new []{self.TypedItemType}, null)
                .Invoke(self, new[] {item});
        }
        
        public static void ActOn(this IActOnItemWithArguments self, IItem item, string arguments)
        {
            self.GetType()
                .GetMethod("ActOn", BindingFlags.Instance | BindingFlags.Public, null, new []{self.TypedItemType, typeof(string)}, null)
                .Invoke(self, new object[] {item, arguments});
        }

        public static ArgumentAutoCompletionResult AutoCompleteArguments(this IActOnItemWithAutoCompletedArguments self, IItem item, string arguments)
        {
            return (ArgumentAutoCompletionResult)self.GetType()
                .GetMethod("AutoCompleteArguments", BindingFlags.Instance | BindingFlags.Public, null, new[] { self.TypedItemType, typeof(string) }, null)
                .Invoke(self, new object[] {item, arguments});
        }

        public static Type GetTypedItemType<T>(this IActOnTypedItem<T> self)
        {
            return typeof (ITypedItem<T>);
        }

        public static Type GetTypedItemType<T>(this IActOnTypedItemWithArguments<T> self)
        {
            return typeof (ITypedItem<T>);
        }
    }

    public interface IActOnTypedItem<in T> : IActOnItem
    {
        void ActOn(ITypedItem<T> item);
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

    public interface IActOnTypedItemWithArguments<in T> : IActOnItemWithArguments
    {
        /// <summary>
        /// Acts on the item with the given arguments
        /// </summary>
        /// <param name="arguments">Can be an empty string</param>
        void ActOn(ITypedItem<T> item, string arguments);
    }

    public interface IActOnTypedItemWithAutoCompletedArguments<in T> : IActOnItemWithArguments
    {
        /// <param name="arguments">Can be empty</param>
        ArgumentAutoCompletionResult AutoCompleteArguments(ITypedItem<T> item, string arguments);
    }
}