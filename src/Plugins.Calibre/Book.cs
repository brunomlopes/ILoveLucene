using System.Collections.Generic;
using Core.API;
using Core.Abstractions;

namespace Plugins.Calibre
{
    public class Book : IItem
    {
        public int Id { get; set; }
        public string Title { get;  set; }
        public string Authors { get; set; }
        public List<string> Formats { get; private set; }

        public Book()
        {
            Formats = new List<string>();
        }

        public string Text
        {
            get { return Title; }
        }

        public string Description
        {
            get { return Text + " - " + Authors; }
        }

        public object Item
        {
            get { return this; }
        }
    }
}