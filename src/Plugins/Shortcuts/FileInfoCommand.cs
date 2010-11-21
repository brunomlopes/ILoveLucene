using System.Diagnostics;
using System.IO;
using Core.Abstractions;

namespace Plugins.Shortcuts
{
    internal class FileInfoCommand : ICommand
    {
        private readonly FileInfo _shortcut;

        public FileInfoCommand(FileInfo shortcut)
        {
            _shortcut = shortcut;
        }

        public string Text { get { return _shortcut.Name; } }
        public string Description { get { return _shortcut.FullName; } }

        public void Execute()
        {
            Process.Start(_shortcut.FullName);
        }
    }
}