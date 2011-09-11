using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceProcess;
using Core.API;
using Core.Abstractions;
using Core.Extensions;
using Lucene.Net.Documents;

namespace Plugins.Services
{
    [Export(typeof(IConverter))]
    public class ServiceItemConverter : IConverter<ServiceController>
    {
        public IItem FromDocumentToItem(CoreDocument document)
        {
            var name = document.GetString("id");
            return new ServiceTypedItem(ServiceController.GetServices().Single(s => s.ServiceName == name));
        }

        public string ToId(ServiceController t)
        {
            return t.ServiceName;
        }

        public CoreDocument ToDocument(IItemSource itemSource, ServiceController t)
        {
            var document = new CoreDocument(itemSource, this, ToId(t), ToName(t), ToType(t));
            document.Store("id", t.ServiceName.ToString());
            return document;
        }

        public string ToName(ServiceController t)
        {
            return t.DisplayName;
        }

        public string ToType(ServiceController t)
        {
            return "service";
        }
    }
}