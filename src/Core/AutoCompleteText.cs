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
        private class Command : ICommand
        {
            public Command(string text)
            {
                Text = text;
            }

            public string Text { get; private set; }
            public string Description { get; private set; }
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

    public class SlowAutoCompleteText : AutoCompleteText
    {
        private string[] words;

        public SlowAutoCompleteText()
        {
            words =
                "Actions feature leverages System.Windows.Interactivity for it’s trigger mechanism fantastic".Split(' ');
        }

        public override AutoCompletionResult Autocomplete(string text)
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
            return base.Autocomplete(text);
        }
    }
}