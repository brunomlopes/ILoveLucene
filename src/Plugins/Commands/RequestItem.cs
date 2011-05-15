using Core.Abstractions;

namespace Plugins.Commands
{
    public class RequestItem : ITypedItem<IRequest>
    {
        private IRequest _item;

        public object Item
        {
            get { return _item; }
        }

        public RequestItem(IRequest command)
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

        public IRequest TypedItem
        {
            get { return _item; }
        }
    }
}