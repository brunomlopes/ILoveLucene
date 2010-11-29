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
        public override void ActOn(ITypedItem<FileInfo> item)
        {
            Process.Start(item.Item.FullName);
        }

        public override string Text
        {
            get { return "Run"; }
        }
    } 
    
    [Export(typeof(IActOnItem))]
    public class RunFileInfoWithArguments : IActOnTypedItemWithArguments<FileInfo>
    {
        public void ActOn(ITypedItem<FileInfo> item, string arguments)
        {
            Process.Start(item.Item.FullName, arguments);
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
        public void ActOn(ITypedItem<FileInfo> item)
        {
            var arguments = new ProcessStartInfo(item.Item.FullName);
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