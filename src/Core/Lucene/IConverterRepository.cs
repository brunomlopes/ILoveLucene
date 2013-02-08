using Core.API;
using Lucene.Net.Documents;

namespace Core.Lucene
{
    public interface IConverterRepository
    {
        CoreDocument ToDocument(IItemSource source, object item);
        IItem FromDocumentToItem(CoreDocument coreDoc);
    }
}