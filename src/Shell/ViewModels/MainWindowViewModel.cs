using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;

namespace ILoveLucene.ViewModels
{
    [Export(typeof(IShell))]
    public class MainWindowViewModel : PropertyChangedBase, IShell
    {
        private string input;

        public string Input
        {
            get { return input; }
            set
            {
                input = value;
                NotifyOfPropertyChange(() => Input);
                NotifyOfPropertyChange(() => CanExecute);
            }
        }

        public bool CanExecute
        {
            get { return !string.IsNullOrWhiteSpace(input); }
        }

        public void Execute()
        {
            MessageBox.Show(string.Format("Hello {0}!", Input)); 
        }
    }
}