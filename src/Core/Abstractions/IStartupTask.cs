using System.Threading.Tasks;

namespace Core.Abstractions
{
    public interface IStartupTask
    {
        Task Execute();
    }
}