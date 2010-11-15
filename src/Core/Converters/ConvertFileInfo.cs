using System;
using System.ComponentModel.Composition;
using System.IO;
using Core.Abstractions;
using Core.AutoCompletes;
using Lucene.Net.Documents;

namespace Core.Converters
{
    [Export(typeof(IConverter))]
    [Export(typeof(IConverter<FileInfo>))]
    class ConvertFileInfo : IConverter<FileInfo>
    {
        public string ToId(FileInfo fileInfo)
        {
            return fileInfo.FullName;
        }

        public string ToName(FileInfo t)
        {
            return Path.GetFileNameWithoutExtension(t.Name);
        }

        public Document ToDocument(FileInfo fileInfo)
        {
            var name = new Field("filename", Path.GetFileNameWithoutExtension(fileInfo.Name), Field.Store.YES,
                                 Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var path = new Field("filepath", fileInfo.FullName, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS);
            var document = new Document();
            document.Add(name);
            document.Add(path);
            return document;
        }

        public ICommand FromDocumentToCommand(Document document)
        {
            return new FileInfoCommand(new FileInfo(document.GetField("filepath").StringValue()));
        }

        public Type ConvertedType
        {
            get { return typeof(FileInfo); }
        }
    }
}