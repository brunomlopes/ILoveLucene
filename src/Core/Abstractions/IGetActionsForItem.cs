using System;
using System.Collections.Generic;
using Core.API;

namespace Core.Abstractions
{
    public interface IGetActionsForItem
    {
        IEnumerable<IActOnItem> ActionsForItem(AutoCompletionResult.CommandResult item);
        void LearnActionForCommandResult(string input, IActOnItem selectedAction, AutoCompletionResult.CommandResult result);
    }
}