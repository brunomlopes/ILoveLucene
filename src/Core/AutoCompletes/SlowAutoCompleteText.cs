using System;
using System.Threading;
using Core.Abstractions;

namespace Core.AutoCompletes
{
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