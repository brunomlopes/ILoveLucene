using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.API;
using Core.Abstractions;

namespace Core
{
    [Export(typeof(IFindDefaultActionForItemStrategy))]
    public class GetDefaultActionBasedOnAttributeForType : IFindDefaultActionForItemStrategy
    {
        private Dictionary<Type, IActOnItem> _actions;

        [ImportMany(typeof(IActOnItem), AllowRecomposition = true)]
        public IEnumerable<IActOnItem> Actions
        {
            get { return _actions.Values; }
            set { _actions = value.ToDictionary(i => i.GetType()); }
        }

        public IActOnItem DefaultForItem(IItem item)
        {
            var defaultActionAttribute = (DefaultActionAttribute)
                                         item.GetType()
                                             .GetCustomAttributes(typeof (DefaultActionAttribute), true)
                                             .SingleOrDefault();
            if (defaultActionAttribute == null)
            {
                return null;
            }
            if (_actions.ContainsKey(defaultActionAttribute.ActionType))
                return _actions[defaultActionAttribute.ActionType];
            return null;
        }
    }
}