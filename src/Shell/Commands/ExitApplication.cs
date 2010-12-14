using System.ComponentModel.Composition;
using System.Windows;
using Core.Abstractions;

namespace ILoveLucene.Commands
{
    [Export(typeof (IItem))]
    [Export(typeof (IActOnItem))]
    public class ExitApplication : BaseCommand<ExitApplication>
    {
        public override void Act()
        {
            Caliburn.Micro.Execute.OnUIThread(() => Application.Current.Shutdown());
        }

        public override string Description
        {
            get { return "Exit application"; }
        }

        public override ExitApplication TypedItem
        {
            get { return this; }
        }
    }
}