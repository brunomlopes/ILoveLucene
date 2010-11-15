using System;
using System.Linq;
using Core.Abstractions;

namespace Core.AutoCompletes
{
    public class AutoCompleteText : IAutoCompleteText
    {
        private class Command : ICommandWithArguments
        {
            public Command(string text)
            {
                Text = text;
            }

            public string Text { get; private set; }
            public string Description { get; private set; }
            public void Execute()
            {
                Execute(string.Empty);
            }

            public void Execute(string arguments)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Executing {0}:{1}", Text, arguments));
            }
        }
        private string[] words;

        public AutoCompleteText()
        {
            words = "Actions feature leverages System.Windows.Interactivity for it’s trigger mechanism fantastic".Split(' ');
        }

        public virtual AutoCompletionResult Autocomplete(string text)
        {
            var completions = words
                .Where(w => w.StartsWith(text, StringComparison.CurrentCultureIgnoreCase))
                .Select(t => new Command(t));
            if (completions.Count() == 0)
            {
                return AutoCompletionResult.NoResult(text);
            }
            return AutoCompletionResult.OrderedResult(text, completions);
        }
    }
}