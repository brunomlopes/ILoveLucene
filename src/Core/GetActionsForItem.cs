using System;
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

        Dictionary<string, IActOnItem> _learnings = new Dictionary<string, IActOnItem>();


        public GetActionsForItem()
        {
            _learnings = new Dictionary<string, IActOnItem>();
        }
        
        public GetActionsForItem(IEnumerable<IActOnItem> actions)
        {
            Actions = actions;
        }

        public IEnumerable<IActOnItem> ActionsForItem(AutoCompletionResult.CommandResult result)
        {
            var item = result.Item;
            var sha1 = result.CompletionId.GetSha1();
            var actions = new List<IActOnItem>();
            if (_learnings.ContainsKey(sha1) && _learnings[sha1].CanActOn(item))
            {
                actions.Add(_learnings[sha1]);
            }
                
            return actions.Concat(Actions.Where(a => a.TypedItemType.IsInstanceOfType(item.Item) && a.CanActOn(item)).OrderBy(c => c.Text)).Distinct();
        }

        public void LearnActionForCommandResult(string input, IActOnItem selectedAction, AutoCompletionResult.CommandResult result)
        {
            _learnings[result.CompletionId.GetSha1()] = selectedAction;
        }
    }
}