using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Core.Abstractions;

namespace Core.AutoCompletes
{
    [Export(typeof(IAutoCompleteText))]
    public class AutoCompleteBasedOnFiles : IAutoCompleteText
    {
        private class Command : ICommand
        {
            private readonly FileInfo _shortcut;

            public Command(FileInfo shortcut)
            {
                _shortcut = shortcut;
            }

            public string Text { get { return _shortcut.Name; } }
            public string Description { get { return _shortcut.FullName; } }

            public void Execute()
            {
                System.Diagnostics.Process.Start(_shortcut.FullName);
            }
        }

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
                    .Select(s => new Command(s));
            return AutoCompletionResult.OrderedResult(text, shortcuts);
        }
    }
}