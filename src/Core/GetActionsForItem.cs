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
            var actions = new List<IActOnItem>();

            if (result.CompletionId != null)
            {
                var sha1 = result.CompletionId.GetSha1();
                if (_learnings.ContainsKey(sha1) && _learnings[sha1].CanActOn(item))
                {
                    actions.Add(_learnings[sha1]);
                }
            }
            var actionForType = ActionForType(item.GetType());
            if(actionForType != null && actionForType.CanActOn(item))
            {
                actions.Add(actionForType);
            }

            return actions.Concat(Actions.Where(a => a.TypedItemType.IsInstanceOfType(item.Item) && a.CanActOn(item)).OrderBy(c => c.Text)).Distinct();
        }

        public void LearnActionForCommandResult(string input, IActOnItem selectedAction, AutoCompletionResult.CommandResult result)
        {
            if (result.IsTransient()) return;
            _learnings[result.CompletionId.GetSha1()] = selectedAction;
            LearnForType(result.Item.GetType(), selectedAction);
        }

        private void LearnForType(Type type, IActOnItem selectedAction)
        {
            var iface = GenericITypedItem(type);
            if (iface != null)
            {
                _learnings[KeyForType(iface)] = selectedAction;
            }
        }

        private IActOnItem ActionForType(Type type)
        {
            var iface = GenericITypedItem(type);
            if (iface != null && _learnings.ContainsKey(KeyForType(iface)))
            {
                return _learnings[KeyForType(iface)];
            }
            return null;
        }
        private Type GenericITypedItem(Type type)
        {
            return type.GetInterfaces().SingleOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof (ITypedItem<>));
        }
        private string KeyForType(Type type)
        {
            return type.GetGenericArguments().First().FullName;
        }
    }
}