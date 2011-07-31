using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using Caliburn.Micro;
using Core.Abstractions;
using Core.Extensions;
using Core.Lucene;
using ILoveLucene.AutoUpdate;
using ILoveLucene.Infrastructure;
using ILoveLucene.Loggers;
using ILoveLucene.Views;
using NAppUpdate.Framework;
using NLog;
using LogManager = NLog.LogManager;

namespace ILoveLucene.ViewModels
{
    public class MainWindowViewModel : PropertyChangedBase
    {
        private readonly AutoCompleteBasedOnLucene _autoCompleteText;
        private readonly IGetActionsForItem _getActionsForItem;
        private readonly Logger _log;
        private readonly UpdateManagerAdapter _updateManager;
        private readonly IWindowManager _windowManager;
        private CancellationTokenSource _cancelationTokenSource;

        [Import]
        public StatusMessage Status { get; set; }

        public MainWindowViewModel(AutoCompleteBasedOnLucene autoCompleteText, IGetActionsForItem getActionsForItem, Logger log, UpdateManagerAdapter updateManager, IWindowManager windowManager)
        {
            _autoCompleteText = autoCompleteText;
            _getActionsForItem = getActionsForItem;
            _log = log;

            _updateManager = updateManager;
            _windowManager = windowManager;
            _updateManager.UpdatesAvailable += (sender, args) =>
                                                   {
                                                       {
                                                           Status.SetMessage(this, "Update available, downloading");
                                                           _updateManager.PrepareUpdates();
                                                       }
                                                   };
            _updateManager.UpdatesReady += (sender, args) =>
                                               {
                                                   Status.SetMessage(this, "Update prepared, ready for install");
                                                   NotifyOfPropertyChange(() => UpdateVisible);
                                                   NotifyOfPropertyChange(() => CanUpdate);
                                               };

            _cancelationTokenSource = new CancellationTokenSource();
            _argumentCancelationTokenSource = new CancellationTokenSource();
            CommandOptions =
                new ListWithCurrentSelection<AutoCompletionResult.CommandResult>(
                    new AutoCompletionResult.CommandResult(new TextItem(string.Empty), null));
            ArgumentOptions = new ListWithCurrentSelection<string>();
            Result = CommandOptions.Current;
        }

        public void Execute(FrameworkElement source)
        {
            Task.Factory.StartNew(() =>
                                      {
                                          try
                                          {
                                              Status.SetMessage(this, "Executing");
                                              IItem result = null;
                                              if (ActionWithArguments != null)
                                              {
                                                  result = ActionWithArguments.ActOn(Result.Item, Arguments);
                                              }
                                              else
                                              {
                                                  result = SelectedAction.ActOn(Result.Item);
                                              }
                                              _autoCompleteText.LearnInputForCommandResult(Input, Result);
                                              _getActionsForItem.LearnActionForCommandResult(Input, SelectedAction, Result);

                                              result = result ?? NoReturnValue.Object;
                                              if(result != NoReturnValue.Object)
                                              {
                                                  _temporaryResults = result.ToListWithCurrentSelection();
                                                  UpdateCommandOptions(new ListWithCurrentSelection<AutoCompletionResult.CommandResult>());
                                                  Input = CommandOptions.First().Item.Text;
                                              }
                                              else
                                              {
                                                  Input = string.Empty;
                                                  Arguments = string.Empty;
                                                  // HACK
                                                  _temporaryResults = new ListWithCurrentSelection<AutoCompletionResult.CommandResult>();
                                                  Caliburn.Micro.Execute.OnUIThread(() => ((MainWindowView)Window.GetWindow(source)).HideWindow());
                                              }
                                              Status.SetMessage(this, "Done");
                                          }
                                          catch (Exception e)
                                          {
                                              Status.SetMessage(this, "Error :" + e.Message);
                                              Description = e.Message;
                                              _log.Error(e);
                                          }
                                      });
        }

        private ListWithCurrentSelection<AutoCompletionResult.CommandResult> _commandOptions;

        public ListWithCurrentSelection<AutoCompletionResult.CommandResult> CommandOptions
        {
            get { return _commandOptions; }
            set
            {
                _commandOptions = value;
                NotifyOfPropertyChange(() => CommandOptions);
            }
        }

        private String _explanation;

        public string Explanation
        {
            get { return _explanation; }
            set
            {
                _explanation = value;
                NotifyOfPropertyChange(() => Explanation);
            }
        }

        private ListWithCurrentSelection<string> _ArgumentOptions;

        public ListWithCurrentSelection<string> ArgumentOptions
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
                return (Item is IActOnItemWithAutoCompletedArguments) && ArgumentOptions.Count > 0
                           ? Visibility.Visible
                           : Visibility.Hidden;
            }
        }

 

        public void Update()
        {
            Status.SetMessage(this, "Applying update");
            _updateManager.ApplyUpdates();
        }
        
        public bool CanUpdate
        {
            get
            {
                return _updateManager.State == UpdateManager.UpdateProcessState.Prepared &&
                       _updateManager.HaveUpdatesAvailable;
            }
        }

        public Visibility UpdateVisible
        {
            get
            {
                return (_updateManager.State == UpdateManager.UpdateProcessState.Prepared &&
                        _updateManager.HaveUpdatesAvailable)
                           ? Visibility.Visible
                           : Visibility.Hidden;
            }
        }



        public void ProcessShortcut(FrameworkElement source, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Escape)
            {
                _temporaryResults = new ListWithCurrentSelection<AutoCompletionResult.CommandResult>();
                ((MainWindowView) Window.GetWindow(source)).Toggle();
                return;
            }

            if(eventArgs.Key == Key.Down || eventArgs.Key == Key.Up)
            {
                if (eventArgs.Key == Key.Down)
                    Result = CommandOptions.Next();
                else
                    Result = CommandOptions.Previous();

                Task.Factory.StartNew(() => SetActionsForResult(Result))
                    .GuardForException(SetError);
                eventArgs.Handled = true;
                return;
            }

            if (eventArgs.KeyboardDevice.Modifiers != ModifierKeys.Control)
            {
                return;
            }

            int index;
            var str = new KeyConverter().ConvertToString(eventArgs.Key);
            if (int.TryParse(str, out index))
            {
                if (index == 0) index = 10;

                index -= 1;
                if (index < CommandOptions.Count)
                {
                    Result = CommandOptions.SetIndex(index);
                    Task.Factory.StartNew(() => SetActionsForResult(Result))
                        .GuardForException(SetError);
                    eventArgs.Handled = true;
                }
            }
        }

        private void SetActionsForResult(AutoCompletionResult.CommandResult result)
        {
            Actions = _getActionsForItem.ActionsForItem(result);
            SelectedAction = Actions.FirstOrDefault();
        }

        public void ProcessArgumentShortcut(FrameworkElement source, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Escape)
            {
                _temporaryResults = new ListWithCurrentSelection<AutoCompletionResult.CommandResult>();
                ((MainWindowView) Window.GetWindow(source)).Toggle();
                eventArgs.Handled = true;
                return;
            }

            if (eventArgs.Key == Key.Down || eventArgs.Key == Key.Up)
            {
                if (eventArgs.Key == Key.Down)
                    Arguments = ArgumentOptions.Next();
                else
                    Arguments = ArgumentOptions.Previous();

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
                    Arguments = ArgumentOptions.SetIndex(index);
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

                                          ListWithCurrentSelection<AutoCompletionResult.CommandResult> options;
                                          if (result.HasAutoCompletion)
                                          {
                                              options = new[] {result.AutoCompletedCommand}
                                                  .Concat(result.OtherOptions)
                                                  .ToListWithCurrentSelection();
                                          }
                                          else
                                          {
                                              options = new TextItem(Input, Description).ToListWithCurrentSelection();
                                                  
                                          }

                                          UpdateCommandOptions(options);
                                      }, token)
                .GuardForException(SetError);
        }
        public void ExplainResult()
        {
            _cancelationTokenSource.Cancel();
            _cancelationTokenSource = new CancellationTokenSource();

            var token = _cancelationTokenSource.Token;
            Task.Factory.StartNew(() =>
                                      {
                                          var result = _autoCompleteText.Autocomplete(Input, true);

                                          token.ThrowIfCancellationRequested();

                                          if (!result.HasAutoCompletion) return;

                                          var commandResults = new[] {result.AutoCompletedCommand}
                                              .Concat(result.OtherOptions).ToList();

                                          Caliburn.Micro.Execute.OnUIThread(
                                              () => new ExplanationView(commandResults)
                                                        .Show());

                                      }, token)
                .GuardForException(SetError);
        }
        
        public void ShowLog()
        {
            var target = LogManager.Configuration.ConfiguredNamedTargets.OfType<BindableCollectionMemoryTarget>().FirstOrDefault();

            _windowManager.ShowWindow(new LogViewModel(target));
        }

        private void UpdateCommandOptions(ListWithCurrentSelection<AutoCompletionResult.CommandResult> options)
        {
            // HACK: this temporaryResults should be a source with results expiring regularly
            var tempResults = _temporaryResults ?? new ListWithCurrentSelection<AutoCompletionResult.CommandResult>();
            if (tempResults.Count > 0)
            {
                options = tempResults.Concat(options).ToListWithCurrentSelection();
            }
            CommandOptions = options;
            Result = CommandOptions.Current;
            ArgumentOptions = new ListWithCurrentSelection<string>();
            Arguments = string.Empty;

            SetActionsForResult(Result);
            AutoCompleteArgument();
        }

        private void SetError(Exception e)
        {
            var aggregateException = e as AggregateException;
            if(aggregateException != null)
            {
                foreach (var exception in aggregateException.InnerExceptions)
                {
                    _log.Error(exception);
                }
            }
            Description = e.Message;
        }

        public void AutoCompleteArgument()
        {
            _argumentCancelationTokenSource.Cancel();
            _argumentCancelationTokenSource = new CancellationTokenSource();
            ArgumentOptions = new ListWithCurrentSelection<string>();

            var token = _argumentCancelationTokenSource.Token;
            var autoCompleteArgumentsCommand = SelectedAction as IActOnItemWithAutoCompletedArguments;
            if (autoCompleteArgumentsCommand == null)
                return;
            Task.Factory.StartNew(() =>
                                      {
                                          var result = autoCompleteArgumentsCommand.AutoCompleteArguments(Item, Arguments);

                                          token.ThrowIfCancellationRequested();
                                          _log.Info("Got autocompletion '{0}' for '{1}' with {2} alternatives",
                                                    result.AutoCompletedArgument, result.OriginalText,
                                                    result.OtherOptions.Count());
                                          if (result.HasAutoCompletion)
                                          {
                                              ArgumentOptions =
                                                  new[] {result.AutoCompletedArgument}
                                                      .Concat(result.OtherOptions)
                                                      .ToListWithCurrentSelection();
                                          }
                                          else
                                          {
                                              ArgumentOptions = new ListWithCurrentSelection<string>(Arguments);
                                          }

                                          Arguments = ArgumentOptions.Current;
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

        public IItem Item
        {
            get { return Result.Item; }
        }

        private AutoCompletionResult.CommandResult _result;

        public AutoCompletionResult.CommandResult Result
        {
            get { return _result; }
            set
            {
                _result = value;
                Description = Item.Description;

                NotifyOfPropertyChange(() => Result);
                NotifyOfPropertyChange(() => Item);
            }
        }

        private IEnumerable<IActOnItem> _actions;
        public IEnumerable<IActOnItem> Actions
        {
            get { return _actions; }
            set
            {
                _actions = value;
                NotifyOfPropertyChange(() => Actions);
            }
        }

        private IActOnItem _selectedAction;
        public IActOnItem SelectedAction
        {
            get { return _selectedAction; }
            set
            {
                _selectedAction = value;
                NotifyOfPropertyChange(() => SelectedAction);
                NotifyOfPropertyChange(() => ArgumentsVisible);
                NotifyOfPropertyChange(() => CanExecute);
            }
        }

        public IActOnItemWithArguments ActionWithArguments
        {
            get { return SelectedAction as IActOnItemWithArguments; }
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
            get { return (Result != null && ActionWithArguments != null)? Visibility.Visible : Visibility.Hidden; }
        }

        private string _input;
        private CancellationTokenSource _argumentCancelationTokenSource;
        private ListWithCurrentSelection<AutoCompletionResult.CommandResult> _temporaryResults;

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
            get { return !string.IsNullOrWhiteSpace(_input) && SelectedAction != null; }
        }
    }
}