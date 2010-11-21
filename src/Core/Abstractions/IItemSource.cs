using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Abstractions
{
    public interface IItemSource
    {
        bool NeedsReindexing { get; }
        Task<IEnumerable<object>> GetItems();
    }
}