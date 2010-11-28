using System;

namespace Core.Abstractions
{
    public interface IItem
    {
        /// <summary>
        /// The text representation of a command that can be found and is autocompleted
        /// </summary>
        string Text { get; }

        string Description { get; }
    }

    public interface ITypedItem<out T> : IItem
    {
        T Item { get; }
    }
}