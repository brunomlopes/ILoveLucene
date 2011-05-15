using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.Abstractions;
using Lucene.Net.Documents;

namespace Plugins.Commands
{
    public abstract class ClassConverter<T> : IConverter<T> where T: class 
    {
        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<T> Items { get; set; }

        public IItem FromDocumentToItem(Document document)
        {
            var fullname = document.GetField("fullname").StringValue();
            var export =
                Items.SingleOrDefault(c => c.GetType().FullName == fullname);

            if (export == null)
                throw new InvalidOperationException(string.Format("Missing Command {0}", fullname));
            return DocumentFromClass(export);
        }

        public string ToId(T t)
        {
            return t.GetType().FullName;
        }

        public Document ToDocument(T t)
        {
            var document = new Document();
            document.Add(new Field("fullname", t.GetType().FullName, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
            return document;
        }

        protected abstract IItem DocumentFromClass(T export);
        public abstract string ToName(T t);
        public abstract string ToType(T t);
    }
}