using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Core.API;
using Core.Abstractions;

namespace Plugins.Calibre.Actions
{
    [Export(typeof(IActOnItem))]
    public class CopyBooksToClipboard : BaseActOnTypedItem<Book>
    {
        [Import]
        public IOnUiThread OnUiThread { get; set; }

        public override void ActOn(Book item)
        {
            var fileDropList = new StringCollection();
            fileDropList.AddRange(item.Formats.Select(f => f.Replace("/", "\\")).ToArray());

            OnUiThread.Execute(() => Clipboard.SetFileDropList(fileDropList));
        }
    }
}