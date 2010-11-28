using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Core.Abstractions;

namespace Plugins
{
    [Export(typeof (IActOnItem))]
    public class Browse : BaseActOnTypedItem<string>, ICanActOnTypedItem<string>
    {
        public override void ActOn(ITypedItem<string> item)
        {
            Uri uri;
            if (Uri.TryCreate(item.Item, UriKind.Absolute, out uri))
            {
                Process.Start(uri.ToString());
            }
            else
            {
                throw new InvalidOperationException("'{0}' doesn't seem like an url");
            }
        }

        public override string Text
        {
            get { return "Browse"; }
        }

        public bool CanActOn(ITypedItem<string> item)
        {
            Uri uri;
            return Uri.TryCreate(item.Item, UriKind.Absolute, out uri);
        }
    }
}