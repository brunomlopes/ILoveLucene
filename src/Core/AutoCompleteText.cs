using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Core.Abstractions;

namespace Core
{
    [Export(typeof(IAutoCompleteText))]
    public class AutoCompleteText : IAutoCompleteText
    {
        private string[] words;

        public AutoCompleteText()
        {
            words = "Actions feature leverages System.Windows.Interactivity for it’s trigger mechanism fantastic".Split(' ');
        }

        public AutoCompletionResult Autocomplete(string text)
        {
            var completions = words.Where(w => w.StartsWith(text, StringComparison.CurrentCultureIgnoreCase));
            if (completions.Count() == 0)
            {
                return AutoCompletionResult.NoResult(text);
            }
            return AutoCompletionResult.OrderedResult(text, completions);
        }
    }

    public class SlowAutoCompleteText : IAutoCompleteText
    {
        private string[] words;

        public SlowAutoCompleteText()
        {
            words =
                "Actions feature leverages System.Windows.Interactivity for it’s trigger mechanism fantastic".Split(' ');
        }

        public AutoCompletionResult Autocomplete(string text)
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
            var completions = words.Where(w => w.StartsWith(text, StringComparison.CurrentCultureIgnoreCase));
            if (completions.Count() == 0)
            {
                return AutoCompletionResult.NoResult(text);
            }
            return AutoCompletionResult.OrderedResult(text, completions);
        }
    }
}