using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly ILog _log;
        private CancellationTokenSource _cancelationTokenSource;

        [ImportingConstructor]
        public MainWindowViewModel(IExecuteCommand executeCommand, IAutoCompleteText autoCompleteText, ILog log)
        {
            _executeCommand = executeCommand;
            _autoCompleteText = autoCompleteText;
            _log = log;
            _cancelationTokenSource = new CancellationTokenSource();
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

        private string _input;
        public string Input
        {
            get { return _input; }
            set
            {
                _input = value;
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

        public void AutoComplete()
        {
            _cancelationTokenSource.Cancel();
            _cancelationTokenSource = new CancellationTokenSource();

            var token = _cancelationTokenSource.Token;
            Task.Factory.StartNew(() =>
                                      {
                                          var result = _autoCompleteText.Autocomplete(Input);

                                          token.ThrowIfCancellationRequested();

                                          _log.Info("Got autocompletion '{0}' for '{1}' with {2} alternatives",
                                                    result.AutoCompletedText, result.OriginalText,
                                                    result.OtherOptions.Count());

                                          if (result.HasAutoCompletion)
                                          {
                                              Command = result.AutoCompletedText;
                                          }
                                      }, token);
        }
    }

    public class HasCompletion
    {
        public AutoCompletionResult Completion { get; set; }
    }
}