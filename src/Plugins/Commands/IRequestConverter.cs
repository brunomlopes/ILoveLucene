using System.ComponentModel.Composition;
using Core.Abstractions;

namespace Plugins.Commands
{
    [Export(typeof (IConverter))]
    public class IRequestConverter : ClassConverter<IRequest>
    {
        protected override IItem DocumentFromClass(IRequest request)
        {
            return new RequestItem(request);
        }

        public override string ToName(IRequest t)
        {
            return t.Text;
        }

        public override string ToType(IRequest t)
        {
            return "Request";
        }
    }
}