using System.Collections.Generic;
using System.Linq;

namespace Core.Abstractions
{
    public class AutoCompletionResult
    {
        public class CommandResult
        {
            public object CompletionId { get; private set; }
            public ICommand Command { get; private set; }

            public CommandResult(ICommand command, object completionId)
            {
                Command = command;
                CompletionId = completionId;
            }
        }

        public bool HasAutoCompletion
        {
            get { return AutoCompletedCommand != null; }
        }

        public string OriginalText { get; private set; }
        public CommandResult AutoCompletedCommand { get; private set; }
        public IEnumerable<CommandResult> OtherOptions { get; private set; }

        protected AutoCompletionResult()
        {
            OtherOptions = new CommandResult[] {};
        }

        public static AutoCompletionResult NoResult(string originalText)
        {
            return new AutoCompletionResult {OriginalText = originalText, AutoCompletedCommand = null};
        }

        public static AutoCompletionResult OrderedResult(string originalText, IEnumerable<CommandResult> result)
        {
            result = result.ToList();
            if (result.Count() == 0)
            {
                return new AutoCompletionResult {OriginalText = originalText, AutoCompletedCommand = null};
            }
            return new AutoCompletionResult
                       {
                           OriginalText = originalText,
                           AutoCompletedCommand = result.First(),
                           OtherOptions = result.Skip(1)
                       };
        }

        public static AutoCompletionResult SingleResult(string text, CommandResult textCommand)
        {
            return OrderedResult(text, new[] {textCommand});
        }
    }
}