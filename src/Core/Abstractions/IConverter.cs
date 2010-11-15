using System;
using Lucene.Net.Documents;
using Lucene.Net.Search.Function;

namespace Core.Abstractions
{
    public interface IConverter
    {
        Type ConvertedType { get; }
        ICommand FromDocumentToCommand(Document document);
    }

    public interface IConverter<in T> : IConverter
    {
        string ToId(T t);
        Document ToDocument(T t);
        string ToName(T t);
    }

    public static class IConverterExtensions
    {
        public static string GetNamespaceForItems(this IConverter self)
        {
            return self.ConvertedType.FullName;
        }
    }
}