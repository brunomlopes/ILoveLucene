using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Core.Extensions;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace Core.API
{
    public class CoreDocument : IEnumerable<IIndexableField>
    {
        private readonly Document _document;
        private string Id {get { return GetString(SpecialFields.Id); }}
        public string ConverterId { get { return GetString(SpecialFields.ConverterId); } }
        private string SourceId { get { return GetString(SpecialFields.SourceId); } }
        private string LearningId { get { return GetString(SpecialFields.LearningId); } }

        /// <summary>
        /// Populates the document from the converter
        /// </summary>
        /// <param name="itemSource">item from which this document was retrieved</param>
        /// <param name="converter">converter identifier</param>
        /// <param name="id">item identifier</param>
        /// <param name="name">item name that is used to be indexed</param>
        /// <param name="type">Type name used in the indexer and later on in the searcher to allow the user to filter by type
        /// Search would be something like "process firefox"</param>
        /// <returns></returns>
        public CoreDocument(IItemSource itemSource, IConverter converter, string id, string name, string type)
        {
            _document = new Document();
            PopulateDocument(itemSource.Id, converter.GetId(), id, name, type);
        }

        public static CoreDocument Rehydrate(Document document)
        {
            Contract.Requires(document != null);
            return new CoreDocument(document);
        }

        private CoreDocument(Document document)
        {
            _document = document;
        }

        public CoreDocument SetLearnings(IEnumerable<string> learnings)
        {
            var field = _document.GetFields(SpecialFields.Learnings);

            if (field != null && field.Length > 0)
            {
                _document.RemoveFields(SpecialFields.Learnings);
            }
            foreach (string learning in learnings.Where(learning => !string.IsNullOrWhiteSpace(learning)))
            {
                _document.Add(FieldForLearning(learning));
            }
            return this;
        }

        public CoreDocument SetLearningId(string id)
        {
            return Store(SpecialFields.LearningId, id);
        }

        /// <summary>
        /// Use this value in the learning hash instead of the item id.
        /// Used by process source to learn based on window title instead of process identifier
        /// </summary>
        public CoreDocument SetItemIdForLearning(string id)
        {
            return Store(SpecialFields.LearningId, Hash.HashStrings(ConverterId, id, SourceId));
        }

        public CoreDocument Tag(string tag)
        {
            return Store(SpecialFields.Tag, tag);
        }

        public static implicit operator Document(CoreDocument document)
        {
            document.RequireFields(SpecialFields.ConverterId, SpecialFields.Id, SpecialFields.SourceId);

            return document._document;
        }

        public CoreDocument Store(string fieldName, params string[] content)
        {
            return SetStringField(fieldName, content);
        }
        
        public CoreDocument Index(string fieldName, params string[] content)
        {
            return SetTextField(fieldName, content);
        }

        public string GetString(string fieldName)
        {
            return (_document.GetField(fieldName)
                    ?? new StringField("name", string.Empty, Field.Store.YES))
                .GetStringValue();
        }

        public IEnumerable<string> GetStringList(string fieldName)
        {
            return _document.GetFields(fieldName).Select(field => field.GetStringValue());
        }

        public DocumentId GetDocumentId()
        {
            RequireFields(SpecialFields.ConverterId, SpecialFields.Id, SpecialFields.SourceId);
            return new DocumentId(ConverterId, Id, SourceId, LearningId);
        }

        private CoreDocument PopulateDocument(string sourceId, string converterId, string id, string name, string type)
        {
            Index(SpecialFields.Name, name);
            Index(SpecialFields.Type, type);
            Store(SpecialFields.Id, id);
            Store(SpecialFields.ConverterId, converterId);
            Store(SpecialFields.SourceId, sourceId);

            Store(SpecialFields.Sha1, new DocumentId(ConverterId, Id, SourceId, LearningId).GetId());
            return this;
        }

        private void RequireFields(params string[] fields)
        {
            foreach (var field in fields.Where(field => _document.GetField(field) == null))
            {
                throw new InvalidOperationException("Document missing required field " + field);
            }
        }
        
        private CoreDocument SetStringField(string fieldName, IEnumerable<string> content)
        {
            // getfield retuns on first, and removefields removes all field by the name
            // removefield would only remove the first with the name
            if (_document.GetField(fieldName) != null)
                _document.RemoveFields(fieldName);
            foreach (var c in content)
            {
                _document.AddStringField(fieldName, c, Field.Store.YES);
            }
            return this;
        }
        private CoreDocument SetTextField(string fieldName, IEnumerable<string> content)
        {
            // getfield retuns on first, and removefields removes all field by the name
            // removefield would only remove the first with the name
            if (_document.GetField(fieldName) != null)
                _document.RemoveFields(fieldName);
            foreach (var c in content)
            {
                _document.AddTextField(fieldName, c, Field.Store.YES);
            }
            return this;
        }

        private static Field FieldForLearning(string learning)
        {
            var field = new TextField(SpecialFields.Learnings, learning, Field.Store.YES)
            {
                Boost = 2
            };
            return field;
        }

        public IEnumerator<IIndexableField> GetEnumerator()
        {
            return _document.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}