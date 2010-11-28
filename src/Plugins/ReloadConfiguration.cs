using System;
using System.ComponentModel.Composition;
using Core.Abstractions;

namespace Plugins
{
    [Export(typeof (IItem))]
    [Export(typeof (IActOnItem))]
    public class ReloadConfiguration : BaseCommand<ReloadConfiguration>
    {
        [Import(AllowRecomposition = true)]
        public ILoadConfiguration LoadConfiguration { get; set; }

        public override void ActOn(ITypedItem<ReloadConfiguration> item)
        {
            LoadConfiguration.Reload();
        }

        public override string Text
        {
            get { return "Reload configuration"; }
        }

        public override string Description
        {
            get { return "Reload configuration from disk"; }
        }

        public override ReloadConfiguration Item
        {
            get { return this; }
        }
    }
}