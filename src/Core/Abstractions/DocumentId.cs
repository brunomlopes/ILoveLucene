using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Lucene.Net.Documents;

namespace Core.Abstractions
{
    public class DocumentId
    {
        public string Namespace { get; private set; }
        public string Id { get; private set; }

        public DocumentId(string ns, string id)
        {
            Namespace = ns;
            Id = id;
        }

        public DocumentId(Document document)
        {
            Namespace = document.GetField("_namespace").StringValue();
            Id = document.GetField("_id").StringValue();
        }

        public string GetSha1()
        {
            var sha1 = SHA1.Create();
            sha1.Initialize();

            return
                BitConverter.ToString(
                    sha1.ComputeHash(Encoding.UTF8.GetBytes(Namespace).Concat(Encoding.UTF8.GetBytes(Id)).ToArray()))
                    .Replace("-", "");
        }

        public override string ToString()
        {
            return String.Format("<Command Namespace:'{0}' Id:'{1}'>", Namespace, Id);
        }
    }
}