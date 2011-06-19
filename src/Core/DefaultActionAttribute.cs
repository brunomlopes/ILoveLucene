using System;
using Core.Abstractions;

namespace Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DefaultActionAttribute : Attribute
    {
        public Type ActionType { get; private set; }

        public DefaultActionAttribute(Type actionType)
        {
            ActionType = actionType;
            if (actionType.GetInterface(typeof(IActOnItem).Name) == null)
            {
                throw new InvalidOperationException("Default action type must implement IActOnItem");
            }
        }
    }
}