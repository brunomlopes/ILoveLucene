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
    public class StandaloneCommandsAndRequestsSource : BaseItemSource
    {
        [ImportMany(AllowRecomposition = true)] 
        public IEnumerable<ICommand> Commands;
        
        [ImportMany(AllowRecomposition = true)] 
        public IEnumerable<IRequest> Requests;

        public override Task<IEnumerable<object>> GetItems()
        {
            //FIXME:  this can be recomposed mid-iteration. we don't want that for now
            var commands = Commands; 
            var requests = Requests; 
            return Task.Factory.StartNew(() => commands.Cast<Object>().Concat(requests));
        }

        public override bool Persistent
        {
            get { return false; }
        }
    }
}