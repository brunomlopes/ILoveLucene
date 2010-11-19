using System;
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
using Core.Extensions;

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
            CommandOptions = new List<AutoCompletionResult.CommandResult>();
            ArgumentOptions = new List<string>();
            Result = new AutoCompletionResult.CommandResult(new TextCommand(string.Empty), null);
        }

        public void Execute(FrameworkElement source)
        {
            try
            {
                if (CommandWithArguments != null)
                {
                    CommandWithArguments.Execute(Arguments);
                }
                else
                {
                    Result.Command.Execute();
                    _autoCompleteText.LearnInputForCommandResult(Input, Result);
                }

                // HACK
                ((MainWindowView)Window.GetWindow(source)).Toggle();
            }
            catch (Exception e)
            {
                Description = e.Message;
                _log.Error(e);
            }
        }

        private IList<AutoCompletionResult.CommandResult> _commandOptions;
        public IList<AutoCompletionResult.CommandResult> CommandOptions
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
                    Result = CommandOptions[index];
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
                if (index < ArgumentOptions.Count)
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
                                              Result = result.AutoCompletedCommand;
                                              CommandOptions = new[] {Result}.Concat(result.OtherOptions).ToList();
                                              Arguments = string.Empty;
                                          }
                                          else
                                          {
                                              Result = new AutoCompletionResult.CommandResult(new TextCommand(Input),
                                                                                              null);
                                              CommandOptions = new List<AutoCompletionResult.CommandResult>();
                                          }
                                      }, token)
                .GuardForException(e => Description = e.Message);

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

                                      }, token)
                .GuardForException(e => Description = e.Message);
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

        public ICommand Command
        {
            get { return Result.Command; }
        }

        private AutoCompletionResult.CommandResult _result;
        public AutoCompletionResult.CommandResult Result
        {
            get { return _result; }
            set
            {
                _result = value;
                Description = Command.Description;

                NotifyOfPropertyChange(() => Result);
                NotifyOfPropertyChange(() => Command);
                NotifyOfPropertyChange(() => ArgumentsVisible);
            }
        }

        public ICommandWithArguments CommandWithArguments
        {
            get { return Command as ICommandWithArguments; }
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
            get { return Result != null && Command is ICommandWithArguments ? Visibility.Visible : Visibility.Hidden; }
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