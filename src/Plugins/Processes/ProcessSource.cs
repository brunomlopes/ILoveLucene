using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.API;
using Core.Abstractions;

namespace Plugins.Processes
{
    [Export(typeof(IItemSource))]
    public class ProcessSource : BaseItemSource
    {
        public override IEnumerable<object> GetItems()
        {
            return Process.GetProcesses();
        }

        public override bool Persistent
        {
            get { return false; }
        }
    }
}