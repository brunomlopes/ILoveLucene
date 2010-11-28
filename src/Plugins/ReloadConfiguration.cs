using System.ComponentModel.Composition;
using Core.Abstractions;

namespace Plugins
{
    [Export(typeof (IItem))]
    public class ReloadConfiguration : IItem
    {
        [Import(AllowRecomposition = true)]
        public ILoadConfiguration LoadConfiguration { get; set; }

        public string Text
        {
            get { return "Reload configuration"; }
        }

        public string Description
        {
            get { return "Reload configuration from disk"; }
        }

        public void Execute()
        {
            LoadConfiguration.Reload();
        }
    }
}