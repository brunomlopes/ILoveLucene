namespace Core.API
{
    public interface IItem
    {
        /// <summary>
        /// The text representation of a command that can be found and is autocompleted
        /// </summary>
        string Text { get; }

        string Description { get; }

        object Item { get; }
    }
}