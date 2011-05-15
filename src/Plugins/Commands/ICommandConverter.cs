using System.ComponentModel.Composition;
using Core.Abstractions;

namespace Plugins.Commands
{
    [Export(typeof (IConverter))]
    public class ICommandConverter : ClassConverter<ICommand>
    {
        protected override IItem DocumentFromClass(ICommand command)
        {
            return new CommandItem(command);
        }

        public override string ToName(ICommand t)
        {
            return t.Text;
        }

        public override string ToType(ICommand t)
        {
            return "Command";
        }
    }
}