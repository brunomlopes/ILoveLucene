using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.Abstractions;

namespace Plugins
{
    [Export(typeof(IItem))]
    public class ShowItemSources : ICommandWithAutoCompletedArguments
    {

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<IItemSource> Sources { get; set; }

        public string Text
        {
            get { return "Show Item Sources"; }
        }

        public string Description
        {
            get { return "Just look at autocompletion :)"; }
        }

        public void Execute()
        {
            return; // NOOP, this is just for the description
        }

        public void Execute(string arguments)
        {
            return;
        }

        public ArgumentAutoCompletionResult AutoCompleteArguments(string arguments)
        {
            return ArgumentAutoCompletionResult.OrderedResult(arguments,
                                                              Sources.Select(s => s.GetType().Name + ":" + s.NeedsReindexing));
        }
    }
}