namespace Core.Abstractions
{
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