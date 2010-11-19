using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Core.Abstractions;

namespace Plugins.Commands
{
    [Export(typeof(IItemSource))]
    public class StandaloneCommandsSource : IItemSource
    {
        private readonly IEnumerable<ICommand> _commands;

        [ImportingConstructor]
        public StandaloneCommandsSource([ImportMany]IEnumerable<ICommand> commands)
        {
            _commands = commands;
        }

        public Task<IEnumerable<object>> GetItems()
        {
            return Task.Factory.StartNew(() => _commands.Cast<Object>());
        }
    }
}