namespace Core.API
{
    public interface IConverter
    {
        IItem FromDocumentToItem(CoreDocument document);
    }

    public interface IConverter<in T> : IConverter
    {
        CoreDocument ToDocument(IItemSource itemSource, T t);
    }
}