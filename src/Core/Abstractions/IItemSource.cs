using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lucene.Net.Store;

namespace Core.Abstractions
{
    public interface IItemSource
    {
        Task<IEnumerable<object>> GetItems();
        string Id { get; }
        /// <summary>
        /// If an itemsource is persistent, then we use a filesystem directory to store the index results.
        /// Otherwise we use an in memory directory
        /// </summary>
        bool Persistent { get; }
    }

    public abstract class BaseItemSource : IItemSource
    {
        public abstract Task<IEnumerable<object>> GetItems();

        public virtual string Id
        {
            get { return GetType().FullName; }
        }

        public virtual bool Persistent
        {
            get { return true; }
        }
    }
}