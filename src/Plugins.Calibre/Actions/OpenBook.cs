using System;
using System.Diagnostics;
using System.Linq;
using Core.Abstractions;

namespace Plugins.Calibre.Actions
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
}