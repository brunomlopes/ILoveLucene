using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Core.Abstractions;
using ILoveLucene.Views;
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
            _argumentCancelationTokenSource = new CancellationTokenSource();
            CommandOptions = new List<ICommand>();
            ArgumentOptions = new List<string>();
        }

        public void Execute(FrameworkElement source)
        {
            if (CommandWithArguments != null)
            {
                CommandWithArguments.Execute(Arguments);
            }
            else
            {
                Command.Execute();
            }

            // HACK
            ((MainWindowView)Window.GetWindow(source)).Toggle();
        }

        private IList<ICommand> _commandOptions;
        public IList<ICommand> CommandOptions
        {
            get { return _commandOptions; }
            set
            {
                _commandOptions = value;
                NotifyOfPropertyChange(() => CommandOptions);
            }
        }
        
        private IList<string> _ArgumentOptions;
        public IList<string> ArgumentOptions
        {
            get { return _ArgumentOptions; }
            set
            {
                _ArgumentOptions = value;
                NotifyOfPropertyChange(() => ArgumentOptions);
                NotifyOfPropertyChange(() => ArgumentOptionsVisibility);
            }
        }

        public Visibility ArgumentOptionsVisibility
        {
            get
            {
                return (Command is ICommandWithArguments) && ArgumentOptions.Count > 0
                           ? Visibility.Visible
                           : Visibility.Hidden;
            }
        }

        public void ProcessShortcut(FrameworkElement source, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Escape)
            {
                ((MainWindowView)Window.GetWindow(source)).Toggle();
                return;
            }

            if (eventArgs.KeyboardDevice.Modifiers != ModifierKeys.Control)
            {
                return;
            }
            
            var str = new KeyConverter().ConvertToString(eventArgs.Key);
            int index;
            if (int.TryParse(str, out index))
            {
                index -= 1;
                if (index < CommandOptions.Count)
                {
                    Command = CommandOptions[index];
                    eventArgs.Handled = true;
                }
            }
            
        }

        public void ProcessArgumentShortcut(FrameworkElement source, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Escape)
            {
                ((MainWindowView)Window.GetWindow(source)).Toggle();
                eventArgs.Handled = true;
                return;
            }

            if (eventArgs.KeyboardDevice.Modifiers != ModifierKeys.Control)
            {
                return;
            }

            var str = new KeyConverter().ConvertToString(eventArgs.Key);
            int index;
            if (int.TryParse(str, out index))
            {
                index -= 1;
                if (index < CommandOptions.Count)
                {
                    Arguments = ArgumentOptions[index];
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
                                              CommandOptions = new []{Command}.Concat(result.OtherOptions).ToList();
                                              Arguments = string.Empty;
                                          }
                                          else
                                          {
                                              Command = new TextCommand(Input);
                                              CommandOptions = new List<ICommand>();
                                          }
                                      }, token);

        }

        public void AutoCompleteArgument()
        {
            _argumentCancelationTokenSource.Cancel();
            _argumentCancelationTokenSource = new CancellationTokenSource();

            var token = _argumentCancelationTokenSource.Token;
            var autoCompleteArgumentsCommand = Command as ICommandWithAutoCompletedArguments;
            if(autoCompleteArgumentsCommand == null)
                return;
            Task.Factory.StartNew(() =>
                                      {
                                          var result = autoCompleteArgumentsCommand.AutoCompleteArguments(Arguments);
                                          
                                          token.ThrowIfCancellationRequested();
                                          _log.Info("Got autocompletion '{0}' for '{1}' with {2} alternatives",
                                                    result.AutoCompletedArgument, result.OriginalText,
                                                    result.OtherOptions.Count());
                                          if (result.HasAutoCompletion)
                                          {
                                              ArgumentOptions =
                                                  new[] {result.AutoCompletedArgument}
                                                      .Concat(result.OtherOptions).ToList();
                                          }
                                          else
                                          {
                                              ArgumentOptions = new List<string>();
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
                Description = Command.Description;

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
            get { return Command != null && Command is ICommandWithArguments ? Visibility.Visible : Visibility.Hidden; }
        }

        private string _input;
        private CancellationTokenSource _argumentCancelationTokenSource;

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