using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Core.Lucene;
using Lucene.Net.Documents;

namespace Core.Abstractions
{
    public class DocumentId
    {
        public string ConverterId { get; private set; }
        public string Id { get; private set; }
        public string SourceId { get; set; }

        public DocumentId(string converterId, string itemId, string sourceId)
        {
            ConverterId = converterId;
            Id = itemId;
            SourceId = sourceId;
        }

        public DocumentId(Document document)
        {
            ConverterId = document.GetField(SpecialFields.ConverterId).StringValue();
            Id = document.GetField(SpecialFields.Id).StringValue();
            SourceId = document.GetField(SpecialFields.SourceId).StringValue();
        }

        public string GetSha1()
        {
            var sha1 = SHA1.Create();
            sha1.Initialize();

            return
                BitConverter.ToString(
                    sha1.ComputeHash(Encoding.UTF8.GetBytes(ConverterId)
                                         .Concat(Encoding.UTF8.GetBytes(Id))
                                         .Concat(Encoding.UTF8.GetBytes(SourceId))
                                         .ToArray()))
                    .Replace("-", "");
        }

        public override string ToString()
        {
            return String.Format("<Command Converter:'{0}' Id:'{1}' Source:'{2}'>", ConverterId, Id, SourceId);
        }
    }
}