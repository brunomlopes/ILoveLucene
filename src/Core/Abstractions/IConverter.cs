using System;
using Lucene.Net.Documents;

namespace Core.Abstractions
{
    public interface IConverter
    {
        Type ConvertedType { get; }
        IItem FromDocumentToItem(Document document);
    }

    public interface IConverter<in T> : IConverter
    {
        string ToId(T t);
        Document ToDocument(T t);
        string ToName(T t);
    }

    public static class IConverterExtensions
    {
        public static string GetId(this IConverter self)
        {
            return self.ConvertedType.FullName;
        }
    }
}