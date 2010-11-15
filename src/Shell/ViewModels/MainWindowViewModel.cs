using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using Core.Abstractions;
using ICommand = Core.Abstractions.ICommand;

namespace ILoveLucene.ViewModels
{
    [Export(typeof(IShell))]
    public class MainWindowViewModel : PropertyChangedBase, IShell
    {
        private readonly IAutoCompleteText _autoCompleteText;
        private readonly ILog _log;
        private CancellationTokenSource _cancelationTokenSource;

        [ImportingConstructor]
        public MainWindowViewModel(IAutoCompleteText autoCompleteText, ILog log)
        {
            _autoCompleteText = autoCompleteText;
            _log = log;
            _cancelationTokenSource = new CancellationTokenSource();
        }

        public void Execute()
        {
            if (CommandWithArguments != null)
            {
                CommandWithArguments.Execute(Arguments);
            }
            else
            {
                Command.Execute();
            }
        }

        private IList<ICommand> _allOptions;
        public IList<ICommand> AllOptions
        {
            get { return _allOptions; }
            set
            {
                _allOptions = value;
                NotifyOfPropertyChange(() => AllOptions);
            }
        }

        public void ProcessShortcut(KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyboardDevice.Modifiers != ModifierKeys.Control)
            {
                return;
            }
            
            var str = new KeyConverter().ConvertToString(eventArgs.Key);
            int index;
            if (int.TryParse(str, out index))
            {
                index -= 1;
                if (index < AllOptions.Count)
                {
                    Command = AllOptions[index];
                    eventArgs.Handled = true;
                }
            }
            
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
                                                    result.AutoCompletedCommand, result.OriginalText,
                                                    result.OtherOptions.Count());

                                          if (result.HasAutoCompletion)
                                          {
                                              Command = result.AutoCompletedCommand;
                                              AllOptions = new []{Command}.Concat(result.OtherOptions).ToList();
                                              Arguments = string.Empty;
                                          }
                                          else
                                          {
                                              Command = new TextCommand(Input);
                                              AllOptions = new List<ICommand>();
                                          }
                                      }, token);

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

        private ICommand _command;
        public ICommand Command
        {
            get { return _command; }
            set
            {
                _command = value;
                NotifyOfPropertyChange(() => Command);
                NotifyOfPropertyChange(() => ArgumentsVisible);
            }
        }

        public ICommandWithArguments CommandWithArguments
        {
            get { return _command as ICommandWithArguments; }
        }

        private string _arguments;
        public string Arguments
        {
            get { return _arguments; }
            set
            {
                _arguments = value;
                NotifyOfPropertyChange(() => Arguments);
            }
        }

        public Visibility ArgumentsVisible
        {
            get
            {
                if (Command != null && Command is ICommandWithArguments)
                {
                    return Visibility.Visible;
                }
                else return Visibility.Hidden;
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
    }
}