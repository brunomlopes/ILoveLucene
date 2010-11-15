using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Abstractions
{
    public interface IItemSource<T>
    {
        Task<IEnumerable<T>> GetItems();
    }
}