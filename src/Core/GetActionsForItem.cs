using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.API;
using Core.Abstractions;
using Core.Extensions;

namespace Core
{
    public class GetActionsForItem : IGetActionsForItem
    {
        [Import]
        public IFindDefaultActionForItemStrategy FindDefaultActionForItemStrategy { get; set; }

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
        
        public GetActionsForItem(IEnumerable<IActOnItem> actions, IFindDefaultActionForItemStrategy findDefaultActionForItemStrategy)
        {
            FindDefaultActionForItemStrategy = findDefaultActionForItemStrategy;
            Actions = actions;
        }

        public IEnumerable<IActOnItem> ActionsForItem(AutoCompletionResult.CommandResult result)
        {
            var item = result.Item;
            var actions = new List<IActOnItem>();

            var defaultActionForItem = DefaultActionForResult(result.Item);
            if(defaultActionForItem != null && defaultActionForItem.CanActOn(item))
            {
                actions.Add(defaultActionForItem);
            }

            var actionForResult = ActionForResult(result);
            if(actionForResult != null && actionForResult.CanActOn(item))
            {
                actions.Add(actionForResult);
            }
            var actionForType = ActionForType(item.GetType());
            if(actionForType != null && actionForType.CanActOn(item))
            {
                actions.Add(actionForType);
            }

            var actionsForItem = Actions
                .Where(a => a.TypedItemType.IsInstanceOfType(item.Item))
                .Where(a => a.CanActOn(item))
                .OrderBy(c => c.Text);
            return actions.Concat(actionsForItem)
                .Distinct();
        }

        private IActOnItem DefaultActionForResult(IItem item)
        {
            if (FindDefaultActionForItemStrategy != null)
            {
                return FindDefaultActionForItemStrategy.DefaultForItem(item);
            }
            return null;
        }

        public void LearnActionForCommandResult(string input, IActOnItem selectedAction, AutoCompletionResult.CommandResult result)
        {
            if (result.IsTransient()) return;
            LearnForResult(result, selectedAction);
            LearnForType(result.Item.GetType(), selectedAction);
        }

        private void LearnForResult(AutoCompletionResult.CommandResult result, IActOnItem selectedAction)
        {
            _learnings[result.CompletionId.GetLearningId()] = selectedAction;
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

        private IActOnItem ActionForResult(AutoCompletionResult.CommandResult result)
        {
            IActOnItem actionForResult = null;
            if (result.CompletionId != null)
            {
                var sha1 = result.CompletionId.GetLearningId();
                if (_learnings.ContainsKey(sha1))
                {
                    actionForResult = _learnings[sha1];
                }
            }
            return actionForResult;
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