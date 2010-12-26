using System;
using System.Collections.Generic;

namespace Core.Abstractions
{
    public interface IGetActionsForItem
    {
        IEnumerable<IActOnItem> ActionsForItem(AutoCompletionResult.CommandResult item);
        void LearnActionForCommandResult(string input, IActOnItem selectedAction, AutoCompletionResult.CommandResult result);
    }
}