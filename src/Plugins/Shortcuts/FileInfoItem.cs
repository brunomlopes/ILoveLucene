using System;
using System.Collections;
using System.IO;
using Core;
using Core.API;
using Core.Abstractions;

namespace Plugins.Shortcuts
{
    [DefaultAction(typeof(Run))]
    public class FileInfoItem : ITypedItem<FileInfo>
    {
        public FileInfoItem(FileInfo shortcut)
        {
            TypedItem = shortcut;
        }

        public string Text
        {
            get { return TypedItem.Name; }
        }

        public string Description
        {
            get { return TypedItem.FullName; }
        }
        public object Item { get { return TypedItem; } }

        public FileInfo TypedItem { get; private set; }
    }
}