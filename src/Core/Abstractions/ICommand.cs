namespace Core.Abstractions
{
    public interface ICommand
    {
        /// <summary>
        /// The text representation of a command that can be found and is autocompleted
        /// </summary>
        string Text { get; }

        string Description { get; }
        void Execute();
    }

    public interface ICommandWithArguments : ICommand
    {
        /// <summary>
        /// Executes the command with the given arguments
        /// </summary>
        /// <param name="arguments">Can be an empty string</param>
        void Execute(string arguments);
    }

    public class TextCommand : ICommand
    {
        public TextCommand(string input)
            : this(input, string.Empty)
        {
        }

        public TextCommand(string input, string description)
        {
            Text = input;
            Description = description;
        }

        public string Text { get; set; }

        public string Description { get; set; }

        public void Execute()
        {
        }
    }

    public interface ICommandWithAutoCompletedArguments : ICommandWithArguments
    {
        /// <param name="arguments">Can be empty</param>
        ArgumentAutoCompletionResult AutoCompleteArguments(string arguments);
    }
}