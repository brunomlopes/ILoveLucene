using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Core.Abstractions;

namespace Plugins.Processes
{
    [Export(typeof(IActOnItem))]
    public class SwitchToWindow : BaseActOnTypedItem<Process>, ICanActOnTypedItem<Process>
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern uint SwitchToThisWindow(IntPtr hwnd, bool isAltTab);

        public override void ActOn(Process item)
        {
            SwitchToThisWindow(item.MainWindowHandle, true);
        }

        public bool CanActOn(Process item)
        {
            return item.MainWindowHandle != IntPtr.Zero;
        }
    }
}