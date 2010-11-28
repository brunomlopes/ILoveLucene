using System.IO;
using Core.Abstractions;

namespace Plugins.Shortcuts
{
    public class FileInfoItem : ITypedItem<FileInfo>
    {
        public FileInfoItem(FileInfo shortcut)
        {
            Item = shortcut;
        }

        public string Text
        {
            get { return Item.Name; }
        }

        public string Description
        {
            get { return Item.FullName; }
        }

        public FileInfo Item { get; private set; }
    }
}