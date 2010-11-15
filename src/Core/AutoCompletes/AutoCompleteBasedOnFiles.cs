using System;
using System.ComponentModel.Composition;
using System.Linq;
using Core.Abstractions;

namespace Core.AutoCompletes
{
    public class AutoCompleteBasedOnFiles : IAutoCompleteText
    {
        private ShortcutFinder _shortcutFinder;

        public AutoCompleteBasedOnFiles()
        {
            _shortcutFinder = new ShortcutFinder();
        }

        public AutoCompletionResult Autocomplete(string text)
        {
            var shortcuts =
                _shortcutFinder.ShortcutPaths
                    .Where(fileInfo => fileInfo.Name.StartsWith(text, StringComparison.CurrentCultureIgnoreCase))
                    .Take(10)
                    .Select(s => new FileInfoCommand(s));
            return AutoCompletionResult.OrderedResult(text, shortcuts);
        }
    }
}