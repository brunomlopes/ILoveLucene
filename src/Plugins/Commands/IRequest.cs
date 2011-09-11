using Core.API;
using Core.Abstractions;

namespace Plugins.Commands
{
    public interface IRequest
    {
        IItem Act();
        string Text { get; }
        string Description { get; }
    }
}