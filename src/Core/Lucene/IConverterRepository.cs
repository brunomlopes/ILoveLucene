using Core.Abstractions;

namespace Core.Lucene
{
    public interface IConverterRepository
    {
        IConverter<T> GetConverterForType<T>();
        IConverter GetConverterForId(string id);
    }
}