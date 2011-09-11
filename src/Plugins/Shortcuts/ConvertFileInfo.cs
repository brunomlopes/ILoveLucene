using System;
using System.ComponentModel.Composition;
using System.IO;
using Core.API;
using Core.Abstractions;
using Core.Extensions;
using Lucene.Net.Documents;

namespace Plugins.Shortcuts
{
    [Export(typeof (IConverter))]
    public class ConvertFileInfo : IConverter<FileInfo>
    {
        public string ToId(FileInfo fileInfo)
        {
            return fileInfo.FullName;
        }

        public string ToName(FileInfo t)
        {
            return Path.GetFileNameWithoutExtension(t.Name);
        }

        public string ToType(FileInfo t)
        {
            return t.FriendlyTypeName();
        }

        public CoreDocument ToDocument(IItemSource itemSource, FileInfo fileInfo)
        {
            var document = new CoreDocument(itemSource, this, ToId(fileInfo), ToName(fileInfo), ToType(fileInfo));
            document.Store("filename", Path.GetFileNameWithoutExtension(fileInfo.Name));
            document.Store("filepath", fileInfo.FullName);
            return document;
        }

        public IItem FromDocumentToItem(CoreDocument document)
        {
            return new FileInfoItem(new FileInfo(document.GetString("filepath")));
        }
    }
}