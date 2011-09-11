using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Core.API;
using Core.Abstractions;
using Core.Extensions;

namespace Plugins.Shortcuts
{
    [Export(typeof (IActOnItem))]
    public class RunWithArguments : IActOnTypedItemWithArguments<FileInfo>
    {
        public void ActOn(FileInfo item, string arguments)
        {
            Process.Start(item.FullName, arguments);
        }

        public string Text
        {
            get { return "Run With arguments"; }
        }

        public Type TypedItemType
        {
            get { return this.GetTypedItemType(); }
        }
    }
}