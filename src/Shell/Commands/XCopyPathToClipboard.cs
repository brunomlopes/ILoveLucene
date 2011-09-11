using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using Core.API;
using Core.Abstractions;

namespace ILoveLucene.Commands
{
    /// <summary>
    /// the X is just so that this command ends up on the end of the box
    /// </summary>
    [Export(typeof(IActOnItem))]
    public class XCopyPathToClipboard : BaseActOnTypedItem<FileInfo>
    {
        [Import]
        public ILog Log { get; set; }

        [Import]
        public IOnUiThread OnUiThread { get; set; }

        public override void ActOn(FileInfo item)
        {
            OnUiThread.Execute(() => Clipboard.SetText(item.FullName));
        }
    }
}