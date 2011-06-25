using System.ComponentModel.Composition;
using Core.Abstractions;
using Plugins.Commands;

namespace Plugins
{
    [Export(typeof (ICommand))]
    public class ReloadConfiguration : BaseCommand
    {
        [Import(AllowRecomposition = true)]
        public ILoadConfiguration LoadConfiguration { get; set; }

        

        public override void Act()
        {
            LoadConfiguration.Load();
        }

        public override string Description
        {
            get { return "Reload configuration from disk"; }
        }
    }
}