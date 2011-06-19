using Core;
using Core.Abstractions;

namespace Plugins.Commands
{
    [DefaultAction(typeof(ExecuteCommand))]
    public class CommandItem : ITypedItem<ICommand>
    {
        private ICommand _item;

        public object Item
        {
            get { return _item; }
        }

        public CommandItem(ICommand command)
        {
            _item = command;
        }

        public string Text
        {
            get { return _item.Text; }
        }

        public string Description
        {
            get { return _item.Description; }
        }

        public ICommand TypedItem
        {
            get { return _item; }
        }
    }
}