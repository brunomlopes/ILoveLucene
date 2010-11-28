using System;
using System.Collections.Generic;

namespace Core.Abstractions
{
    public interface IGetActionsForItem
    {
        IEnumerable<IActOnItem> ActionsForItem(IItem item);
    }
}