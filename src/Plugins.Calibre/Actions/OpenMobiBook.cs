using System;
using System.ComponentModel.Composition;
using Core.Abstractions;

namespace Plugins.Calibre.Actions
{
    [Export(typeof(IActOnItem))]
    public class OpenMobiBook : OpenBook
    {
        protected override Func<string, bool> FormatSelector
        {
            get { return f => f.ToLowerInvariant().EndsWith(".mobi");}
        }
    }
}