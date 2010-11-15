using System;

namespace Core.Abstractions
{
    public interface ICommand
    {
        /// <summary>
        /// The text representation of a command that can be found and is autocompleted
        /// </summary>
        string Text { get; }
        string Description { get; }
        void Execute(string arguments);
    }

    public class TextCommand : ICommand
    {
        public TextCommand(string input)
        {
            Text = input;
        }

        public string Text { get; set; }

        public string Description
        {
            get { return String.Empty; }
        }

        public void Execute(string arguments)
        {
        }
    }
}