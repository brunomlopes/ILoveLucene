using System.ComponentModel.Composition;
using System.Windows;
using Core.Abstractions;

namespace ILoveLucene.Commands
{
    [Export(typeof (ICommand))]
    public class ExitApplication : ICommand
    {
        public string Text
        {
            get { return "Exit"; }
        }

        public string Description
        {
            get { return "Exit application"; }
        }

        public void Execute()
        {
            Application.Current.Shutdown();
        }
    }
}