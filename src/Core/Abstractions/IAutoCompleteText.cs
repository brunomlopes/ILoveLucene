namespace Core.Abstractions
{
    public interface IAutoCompleteText
    {
        AutoCompletionResult Autocomplete(string text);
        void LearnInputForCommandResult(string input, AutoCompletionResult.CommandResult result);
    }
}