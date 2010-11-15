using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using Core.Abstractions;

namespace ILoveLucene.Commands
{
    public class ExitApplication : ICommand
    {
        [Import]
        public IWindowManager WindowManager { get; set; }

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