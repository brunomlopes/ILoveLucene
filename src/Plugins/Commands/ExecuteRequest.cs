using System;
using System.ComponentModel.Composition;
using Core.Abstractions;
using Core.Extensions;

namespace Plugins.Commands
{
    [Export(typeof(IActOnItem))]
    public class ExecuteRequest : IActOnTypedItemAndReturnItem<IRequest>
    {
        public IItem ActOn(IRequest item)
        {
            return item.Act();
        }

        public string Text
        {
            get { return this.FriendlyTypeName(); }
        }

        public Type TypedItemType
        {
            get { return typeof(IRequest); }
        }
    }
}