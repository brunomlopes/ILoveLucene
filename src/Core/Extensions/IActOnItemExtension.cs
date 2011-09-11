using System;
using System.Reflection;
using Core.API;
using Core.Abstractions;

namespace Core.Extensions
{
    public static class IActOnItemExtension
    {
        public static IItem ActOn(this IActOnItem self, IItem item)
        {
            IItem returnedValue = null;
            MethodInfo methodInfo = self.GetType()
                .GetMethod("ActOn", BindingFlags.Instance | BindingFlags.Public, null, new[] {self.TypedItemType}, null);
            UnwrapTargetInvocationException(() =>
                                            returnedValue = methodInfo.Invoke(self, new[] {item.Item}) as IItem);
            if (methodInfo.ReturnType == typeof (void))
            {
                returnedValue = NoReturnValue.Object;
            }
            return returnedValue;
        }

        public static IItem ActOn(this IActOnItemWithArguments self, IItem item, string arguments)
        {
            IItem returnedValue = null;
            MethodInfo methodInfo = self.GetType()
                .GetMethod("ActOn", BindingFlags.Instance | BindingFlags.Public, null,
                           new[] {self.TypedItemType, typeof (string)}, null);
            UnwrapTargetInvocationException(() =>
                                            returnedValue = methodInfo.Invoke(self, new object[] {item.Item, arguments}) as IItem);
            if (methodInfo.ReturnType == typeof (void))
            {
                returnedValue = NoReturnValue.Object;
            }
            return returnedValue;
        }

        public static ArgumentAutoCompletionResult AutoCompleteArguments(this IActOnItemWithAutoCompletedArguments self, IItem item, string arguments)
        {
            return UnwrapTargetInvocationException(() => (ArgumentAutoCompletionResult)self.GetType()
                                                     .GetMethod("AutoCompleteArguments", BindingFlags.Instance | BindingFlags.Public, null, new[] { self.TypedItemType, typeof(string) }, null)
                                                     .Invoke(self, new object[] {item.Item, arguments}));
        }
        
        public static bool CanActOn(this IActOnItem self, IItem item)
        {
            if (!(self is ICanActOnItem))
                return true;

            return (bool)self.GetType()
                             .GetMethod("CanActOn", BindingFlags.Instance | BindingFlags.Public, null, new[] { self.TypedItemType}, null)
                             .Invoke(self, new object[] {item.Item});
        }

        private static void UnwrapTargetInvocationException(Action action)
        {
            try
            {
                action();
            }
            catch(TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        private static T UnwrapTargetInvocationException<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public static Type GetTypedItemType<T>(this IActOnTypedItem<T> self)
        {
            return typeof (T);
        }

        public static Type GetTypedItemType<T>(this IActOnTypedItemWithArguments<T> self)
        {
            return typeof (T);
        }

        public static Type GetTypedItemType<T, TItem>(this IActOnTypedItemAndReturnTypedItem<T,TItem> self)
        {
            return typeof (T);
        }
    }
}