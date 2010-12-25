using System;
using System.Diagnostics;
using Core.Abstractions;

namespace Plugins.Processes
{
    internal class ProcessItem : ITypedItem<Process>
    {
        private readonly Process _process;

        public ProcessItem(Process process)
        {
            _process = process;
        }

        public string Text
        {
            get { return _process.ProcessName; }
        }

        public string Description
        {
            get { return _process.ProcessName + "-" + _process.MainWindowTitle; }
        }

        public object Item
        {
            get { return TypedItem; }
        }

        public Process TypedItem
        {
            get { return _process; }
        }
    }
}