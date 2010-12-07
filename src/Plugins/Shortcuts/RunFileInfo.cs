using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Core.Abstractions;

namespace Plugins.Shortcuts
{
    [Export(typeof(IActOnItem))]
    public class Run : BaseActOnTypedItem<FileInfo>
    {
        public override void ActOn(FileInfo item)
        {
            Process.Start(item.FullName);
        }
    } 
    
    [Export(typeof(IActOnItem))]
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
    
    [Export(typeof(IActOnItem))]
    public class RunElevated : BaseActOnTypedItem<FileInfo>
    {
        public override void ActOn(FileInfo item)
        {
            var arguments = new ProcessStartInfo(item.FullName);
            arguments.Verb = "runas";
            Process.Start(arguments);
        }
    }
}