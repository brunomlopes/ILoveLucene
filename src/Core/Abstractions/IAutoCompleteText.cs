namespace Core.Abstractions
{
    public interface IAutoCompleteText
    {
        AutoCompletionResult Autocomplete(string text);
    }
}