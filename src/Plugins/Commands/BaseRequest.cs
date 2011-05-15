using Core.Abstractions;
using Core.Extensions;

namespace Plugins.Commands
{
    public abstract class BaseRequest : IRequest
    {
        public abstract IItem Act();

        public virtual string Text
        {
            get { return this.FriendlyTypeName(); }
        }

        public virtual string Description
        {
            get { return Text + " command"; }
        }
    }
}