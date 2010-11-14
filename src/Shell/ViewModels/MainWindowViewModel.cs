using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using Core.Abstractions;

namespace ILoveLucene.ViewModels
{
    [Export(typeof(IShell))]
    public class MainWindowViewModel : PropertyChangedBase, IShell
    {
        private readonly IExecuteCommand _executeCommand;
        private readonly IAutoCompleteText _autoCompleteText;
        private string _input;

        [ImportingConstructor]
        public MainWindowViewModel(IExecuteCommand executeCommand, IAutoCompleteText autoCompleteText)
        {
            _executeCommand = executeCommand;
            _autoCompleteText = autoCompleteText;
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        private string _command;
        public string Command
        {
            get { return _command; }
            set
            {
                _command = value;
                NotifyOfPropertyChange(() => Command);
            }
        }

        public string Input
        {
            get { return _input; }
            set
            {
                _input = value;
                
                var autoCompletionResult = _autoCompleteText.Autocomplete(_input);
                if (autoCompletionResult.HasAutoCompletion)
                {
                    Command = autoCompletionResult.AutoCompletedText;
                }

                NotifyOfPropertyChange(() => Input);
                NotifyOfPropertyChange(() => CanExecute);
            }
        }

        public bool CanExecute
        {
            get { return !string.IsNullOrWhiteSpace(_input); }
        }

        public void Execute()
        {
            _executeCommand.Execute(_input);
        }
    }
}