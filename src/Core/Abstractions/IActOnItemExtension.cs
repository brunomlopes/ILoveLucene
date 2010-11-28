using System;
using System.Reflection;

namespace Core.Abstractions
{
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
        
        public static bool CanActOn(this IActOnItem self, IItem item)
        {
            if (!(self is ICanActOnItem))
                return true;

            return (bool)self.GetType()
                             .GetMethod("CanActOn", BindingFlags.Instance | BindingFlags.Public, null, new[] { self.TypedItemType}, null)
                             .Invoke(self, new object[] {item});
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
}