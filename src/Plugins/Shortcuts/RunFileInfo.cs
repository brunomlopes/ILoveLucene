using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Core.Abstractions;

namespace Plugins.Shortcuts
{
    [Export(typeof(IActOnItem))]
    public class RunFileInfo : BaseActOnTypedItem<FileInfo>
    {
        public override void ActOn(FileInfo item)
        {
            Process.Start(item.FullName);
        }

        public override string Text
        {
            get { return "Run"; }
        }
    } 
    
    [Export(typeof(IActOnItem))]
    public class RunFileInfoWithArguments : IActOnTypedItemWithArguments<FileInfo>
    {
        public void ActOn(FileInfo item, string arguments)
        {
            Process.Start(item.FullName, arguments);
        }

        public string Text
        {
            get { return "Run with arguments"; }
        }
        public Type TypedItemType
        {
            get { return this.GetTypedItemType(); }
        }
    } 
    
    [Export(typeof(IActOnItem))]
    public class RunFileInfoElevated : IActOnTypedItem<FileInfo>
    {
        public void ActOn(FileInfo item)
        {
            var arguments = new ProcessStartInfo(item.FullName);
            arguments.Verb = "runas";
            Process.Start(arguments);
        }

        public string Text
        {
            get { return "Run elevated"; }
        }
        public Type TypedItemType
        {
            get { return this.GetTypedItemType(); }
        }
    }
}