using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Threading.Tasks;
using Core.Abstractions;

namespace Plugins.Commands
{
    [Export(typeof (IItemSource))]
    public class StandaloneCommandsSource : BaseItemSource
    {
        [ImportMany(AllowRecomposition = true)] 
        public IEnumerable<ICommand> Commands;

        public override Task<IEnumerable<object>> GetItems()
        {
            var commands = Commands; //FIXME:  this can be recomposed mid-iteration. we don't want that for now
            return Task.Factory.StartNew(() => commands.Cast<Object>());
        }

        public override bool Persistent
        {
            get { return false; }
        }
    }
}