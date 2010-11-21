using System;
using System.ComponentModel.Composition;
using Core.Abstractions;

namespace Plugins.Shortcuts
{
    public class ReindexShortcutSource : ICommand
    {
        [Import(typeof(ShortcutSource))]
        public ShortcutSource ShortcutSource { get; set; }
        
        [Import(typeof(IIndexer))]
        public IIndexer Indexer { get; set; }

        public string Text
        {
            get { return "Re-index shortcut source"; }
        }

        public string Description
        {
            get { return "Force re-index of shortcuts"; }
        }

        public void Execute()
        {
            ShortcutSource.NeedsReindexing = true;
            Indexer.Index();
        }
    }
}