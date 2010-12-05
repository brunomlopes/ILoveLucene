using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Core.Abstractions;

namespace Core
{
    public class GetActionsForItem : IGetActionsForItem
    {
        [ImportMany(typeof(IActOnItem), AllowRecomposition = true)]
        public IEnumerable<IActOnItem> Actions { get; set; }

        public GetActionsForItem(CompositionContainer container)
        {
            container.SatisfyImportsOnce(this);
        }
        
        public GetActionsForItem(IEnumerable<IActOnItem> actions)
        {
            Actions = actions;
        }

        public IEnumerable<IActOnItem> ActionsForItem(IItem item)
        {
            return Actions.Where(a => a.TypedItemType.IsInstanceOfType(item.Item) && a.CanActOn(item)).ToList();
        }
    }
}