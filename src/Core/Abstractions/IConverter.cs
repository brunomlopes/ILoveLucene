using Lucene.Net.Documents;

namespace Core.Abstractions
{
    public interface IConverter
    {
        IItem FromDocumentToItem(Document document);
    }

    public interface IConverter<in T> : IConverter
    {
        string ToId(T t);
        Document ToDocument(T t);
        string ToName(T t);

        /// <summary>
        /// Type name used in the indexer and later on in the searcher to allow the user to filter by type
        /// Search would be something like "process firefox"
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        string ToType(T t);
    }

    public static class IConverterExtensions
    {
        public static string GetId(this IConverter self)
        {
            return self.GetType().FullName;
        }
    }
}