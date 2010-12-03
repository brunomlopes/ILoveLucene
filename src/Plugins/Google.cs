using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Core.Abstractions;

namespace Plugins
{
    [Export(typeof (IActOnItem))]
    public class Google : BaseActOnTypedItem<string>
    {
        public override void ActOn(ITypedItem<string> item)
        {
            var uri = new Uri("http://www.google.com/search?q=" + Uri.EscapeDataString(item.Item), UriKind.Absolute);
            Process.Start(uri.ToString());
        }

        public override string Text
        {
            get { return "Google"; }
        }
    }
}