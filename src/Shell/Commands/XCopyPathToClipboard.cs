using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using Core.Abstractions;

namespace ILoveLucene.Commands
{
    /// <summary>
    /// the X is just so that this command ends up on the end of the box
    /// </summary>
    [Export(typeof(IActOnItem))]
    public class XCopyPathToClipboard : BaseActOnTypedItem<FileInfo>
    {
        public override void ActOn(FileInfo item)
        {
            Caliburn.Micro.Execute.OnUIThread(() => Clipboard.SetText(item.FullName));
        }
    }
}