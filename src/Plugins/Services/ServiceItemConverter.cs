using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceProcess;
using Core.Abstractions;
using Core.Extensions;
using Lucene.Net.Documents;

namespace Plugins.Services
{
    [Export(typeof(IConverter))]
    public class ServiceItemConverter : IConverter<ServiceController>
    {
        public IItem FromDocumentToItem(Document document)
        {
            var name = document.String("id");
            return new ServiceTypedItem(ServiceController.GetServices().Single(s => s.ServiceName == name));
        }

        public string ToId(ServiceController t)
        {
            return t.ServiceName;
        }

        public Document ToDocument(ServiceController t)
        {
            return this.Document().Store("id", t.ServiceName);
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