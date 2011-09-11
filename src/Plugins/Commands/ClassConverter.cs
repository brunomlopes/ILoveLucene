using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.API;
using Core.Abstractions;
using Lucene.Net.Documents;

namespace Plugins.Commands
{
    public abstract class ClassConverter<T> : IConverter<T> where T: class 
    {
        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<T> Items { get; set; }

        public IItem FromDocumentToItem(CoreDocument document)
        {
            var fullname = document.GetString("fullname");
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

        public CoreDocument ToDocument(IItemSource itemSource, T t)
        {
            var document = new CoreDocument(itemSource, this, ToId(t), ToName(t), ToType(t));
            document.Store("fullname", t.GetType().FullName);
            return document;
        }

        protected abstract IItem DocumentFromClass(T export);
        public abstract string ToName(T t);
        public abstract string ToType(T t);
    }
}