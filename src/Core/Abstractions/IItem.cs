using System;

namespace Core.Abstractions
{
    public interface IItem
    {
        /// <summary>
        /// The text representation of a command that can be found and is autocompleted
        /// </summary>
        string Text { get; }

        string Description { get; }
    }

    public interface ITypedItem<out T> : IItem
    {
        T Item { get; }
    }

    public class TextItem : ITypedItem<string>
    {
        public TextItem(string input)
            : this(input, string.Empty)
        {
        }

        public TextItem(string input, string description)
        {
            Text = input;
            Description = description;
        }

        public string Text { get; set; }

        public string Description { get; set; }

        public void Execute()
        {
        }

        public string Item
        {
            get { return Text; }
        }
    }

}