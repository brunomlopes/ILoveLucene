using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;

namespace Core.API
{
    public class CoreDocument
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


        public void Tag(string tag)
        {
            Store(SpecialFields.Tag, tag);
        }

        public static implicit operator Document(CoreDocument document)
        {
            document.RequireFields(SpecialFields.ConverterId, SpecialFields.Id, SpecialFields.SourceId);

            return document._document;
        }

        public CoreDocument Store(string fieldName, params string[] content)
        {
            return SetField(fieldName, content, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO); ;
        }
        
        public CoreDocument Index(string fieldName, params string[] content)
        {
            return SetField(fieldName, content, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
        }

        public string GetString(string fieldName)
        {
            return (_document.GetField(fieldName)
                    ?? new Field("name", string.Empty, Field.Store.YES, Field.Index.NO))
                .StringValue();
        }

        public IEnumerable<string> GetStringList(string fieldName)
        {
            return _document.GetFields(fieldName).Select(field => field.StringValue());
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

        private CoreDocument SetField(string fieldName, IEnumerable<string> content, Field.Index analyzed, Field.TermVector withTermVector)
        {
            // getfield retuns on first, and removefields removes all field by the name
            // removefield would only remove the first with the name
            if (_document.GetField(fieldName) != null)
                _document.RemoveFields(fieldName);
            foreach (var c in content)
            {
                _document.Add(new Field(fieldName, c, Field.Store.YES, analyzed,
                                        withTermVector));
            }
            return this;
        }

        private static Field FieldForLearning(string learning)
        {
            var field = new Field(SpecialFields.Learnings, learning, Field.Store.YES,
                                  Field.Index.ANALYZED,
                                  Field.TermVector.WITH_POSITIONS_OFFSETS);
            field.SetBoost(2);
            return field;
        }
    }
}