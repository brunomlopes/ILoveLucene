using System.ComponentModel.Composition;
using Core.API;
using Core.Abstractions;

namespace Plugins.Commands
{
    [Export(typeof(IActOnItem))]
    public class ExecuteCommand : BaseActOnTypedItem<ICommand>
    {
        public override void ActOn(ICommand item)
        {
            item.Act();
        }
    }
}