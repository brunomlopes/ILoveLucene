using Core.API;

namespace Plugins.OneNote
{
    public class OneNotePage : IItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SectionNodePath { get; set; }
        public string SectionName { get; set; }

        public string Text
        {
            get { return SectionName + " - " + Name; }
        }

        public string Description
        {
            get { return Text + " - " + SectionNodePath; }
        }

        public object Item
        {
            get { return this; }
        }
    }
}