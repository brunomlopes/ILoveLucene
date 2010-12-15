using System;
using Core.Extensions;

namespace Plugins.Commands
{
    public abstract class BaseCommand : ICommand
    {
        public abstract void Act();

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