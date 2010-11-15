using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Abstractions
{
    public interface IItemSource
    {
        Task<IEnumerable<object>> GetItems();
    }
}