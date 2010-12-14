using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Core.Abstractions;

namespace Plugins
{
    [Export(typeof(IItem))]
    [Export(typeof(IActOnItem))]
    public class EmptyRecycleBin : BaseCommand<EmptyRecycleBin>
    {
        enum RecycleFlags : uint
        {
            SHERB_NOCONFIRMATION = 0x00000001,
            SHERB_NOPROGRESSUI = 0x00000002,
            SHERB_NOSOUND = 0x00000004
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);

        public override void Act()
        {
            SHEmptyRecycleBin(IntPtr.Zero, null, 0);
        }

        public override string Text
        {
            get { return "Empty recycle bin"; }
        }

        public override string Description
        {
            get { return Text; }
        }

        public override EmptyRecycleBin TypedItem
        {
            get { return this; }
        }
    }
}