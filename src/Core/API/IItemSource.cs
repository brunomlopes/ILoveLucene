using System.Collections.Generic;

namespace Core.API
{
    public interface IItemSource
    {
        IEnumerable<object> GetItems();
        string Id { get; }
        /// <summary>
        /// If an itemsource is persistent, then we use a filesystem directory to store the index results.
        /// Otherwise we use an in memory directory
        /// </summary>
        bool Persistent { get; }
    }
}