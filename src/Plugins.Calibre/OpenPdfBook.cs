using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Core.Abstractions;

namespace Plugins.Calibre
{
    public abstract class OpenBook : BaseActOnTypedItem<Book>, ICanActOnTypedItem<Book>
    {
        protected abstract Func<string, bool> FormatSelector { get; }

        public override void ActOn(Book item)
        {
            Process.Start(item.Formats.Single(FormatSelector));
        }

        public bool CanActOn(Book item)
        {
            return item.Formats.Any(FormatSelector);
        }
    }

    [Export(typeof(IActOnItem))]
    public class OpenEpubBook : OpenBook
    {
        protected override Func<string, bool> FormatSelector
        {
            get { return f => f.ToLowerInvariant().EndsWith(".mobi");}
        }
    }

    [Export(typeof (IActOnItem))]
    public class OpenPdfBook : OpenBook
    {
        protected override Func<string, bool> FormatSelector
        {
            get { return f => f.ToLowerInvariant().EndsWith(".pdf"); }
        }
    }

    [Export(typeof(IActOnItem))]
    public class OpenMobiBook : OpenBook
    {
        protected override Func<string, bool> FormatSelector
        {
            get { return f => f.ToLowerInvariant().EndsWith(".epub");}
        }
    }
    
}