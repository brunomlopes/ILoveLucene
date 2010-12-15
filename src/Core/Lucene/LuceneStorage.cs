using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Core.Abstractions;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Core.Lucene
{
    public class LuceneStorage
    {
        class LearningStorage
        {
            public LearningStorage(DirectoryInfo input)
            {
                _rootDirectory = input;
                if(!input.Exists)
                {
                    input.Create();
                    input.Refresh();
                }

                var dirs = input.EnumerateDirectories("??");
                _learnings = dirs
                    .SelectMany(d =>d.EnumerateFiles().Select(f => new {f.Name, Text = File.ReadAllText(f.FullName).Trim()}))
                    .ToDictionary(t => t.Name, t => t.Text);
            }

            public string LearningsFor(DocumentId id)
            {
                var sha1 = id.GetSha1();
                if (_learnings.ContainsKey(sha1))
                {
                    return _learnings[sha1];
                }
                return string.Empty;
            }

            public string LearnFor(string learning, DocumentId id)
            {
                var sha1 = id.GetSha1();
                if(!_learnings.ContainsKey(sha1))
                    _learnings[sha1] = learning;
                else
                    _learnings[sha1] += " " + learning;
                WriteLearning(sha1, _learnings[sha1]);
                return _learnings[sha1];
            }

            public void SaveAll()
            {
                foreach (var learning in _learnings)
                {
                    var sha1 = learning.Key;
                    var learningValue = learning.Value;
                    WriteLearning(sha1, learningValue);
                }
            }

            private void WriteLearning(string sha1, string learningValue)
            {
                var path = SubPathFor(sha1);
                if(!Directory.Exists(path))
                    _rootDirectory.CreateSubdirectory(path);

                var fullPath = Path.Combine(_rootDirectory.FullName, path, sha1);
                File.WriteAllText(fullPath, learningValue);
            }

            private string SubPathFor(string sha1)
            {
                return sha1.Substring(0, 1);
            }

            private readonly Dictionary<string, string> _learnings;

            private readonly DirectoryInfo _rootDirectory;
        }

        private class DocumentId
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
                return string.Format("<Command Namespace:'{0}' Id:'{1}'>", Namespace, Id);
            }
        }

        private Dictionary<string, IConverter> _convertersForNamespaces;
        private LearningStorage _learningStorage;

        public LuceneStorage(IEnumerable<IConverter> converters, DirectoryInfo storageLocation)
        {
            SetConverters(converters);
            _learningStorage = new LearningStorage(storageLocation);
        }

        public void UpdateDocumentForObject(IndexWriter writer, IItemSource source, string tag, object item)
        {
            var type = item.GetType();
            GetType().GetMethod("UpdateDocumentForItem").MakeGenericMethod(type)
                .Invoke(this, new[] {writer, source, tag, item});
        }

        public void UpdateDocumentForItem<T>(IndexWriter writer, IItemSource source, string tag, T item)
        {
            var converter = GetConverter<T>();
            var nspace = converter.GetNamespaceForItems();
            var id = converter.ToId(item);

            var documentId = new DocumentId(nspace, id);
            var sha1 = documentId.GetSha1();


            var oldDocument = PopDocument(writer, sha1); //deleting the old version of the doc


            var learnings = _learningStorage.LearningsFor(documentId);

            var name = converter.ToName(item);
            var document = converter.ToDocument(item);
            var sourceId = SourceId(source);

            document.Add(new Field(SpecialFields.Id, id, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field(SpecialFields.Name, name, Field.Store.YES,
                                   Field.Index.ANALYZED,
                                   Field.TermVector.WITH_POSITIONS_OFFSETS));
            document.Add(new Field(SpecialFields.Learnings, learnings, Field.Store.YES,
                                   Field.Index.ANALYZED,
                                   Field.TermVector.WITH_POSITIONS_OFFSETS));
            document.Add(new Field(SpecialFields.Namespace, nspace, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field(SpecialFields.SourceId, sourceId, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field(SpecialFields.Tag, tag, Field.Store.YES,
                                   Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            document.Add(new Field(SpecialFields.Sha1, sha1, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS,
                                   Field.TermVector.NO));
            writer.AddDocument(document);
        }

        public string SourceId(IItemSource source)
        {
            return source.GetType().FullName;
        }

        public AutoCompletionResult.CommandResult GetCommandResultForDocument(Document document)
        {
            var nspace = document.GetField(SpecialFields.Namespace).StringValue();
            if (!_convertersForNamespaces.ContainsKey(nspace))
            {
                throw new NotImplementedException(string.Format("No converter for namespace {0} found", nspace));
            }
            var command = _convertersForNamespaces[nspace].FromDocumentToItem(document);

            return new AutoCompletionResult.CommandResult(command, new DocumentId(document));
        }

        public void LearnCommandForInput(IndexWriter writer, object commandIdObject, string input)
        {
            // fickle command, isn't learnable
            if (commandIdObject == null) return;

            if (!(commandIdObject is DocumentId))
                throw new InvalidOperationException(
                    "Id is not DocumentId. It means the command didn't originate from this class");

            var commandId = (DocumentId) commandIdObject;
            var document = PopDocument(writer, commandId.GetSha1());

            if (document == null)
                throw new InvalidOperationException(string.Format("Didn't find command {0}", commandId));

            var learnings = _learningStorage.LearnFor(input, commandId);

            var field = document.GetField(SpecialFields.Learnings);
            if (field != null)
            {
                document.RemoveField(SpecialFields.Learnings);
            }
            var newField = new Field(SpecialFields.Learnings, learnings, Field.Store.YES, Field.Index.ANALYZED);
                
            document.Add(newField);

            writer.AddDocument(document);
        }

        public void DeleteDocumentsForSourceWithoutTag(IndexWriter indexWriter, IItemSource source, string tag)
        {
            var query = new BooleanQuery();
            query.Add(new BooleanClause(new TermQuery(new Term(SpecialFields.SourceId, SourceId(source))),
                                        BooleanClause.Occur.MUST));
            query.Add(new BooleanClause(new TermQuery(new Term(SpecialFields.Tag, tag)),
                                        BooleanClause.Occur.MUST_NOT));
            indexWriter.DeleteDocuments(query);
        }

        private IConverter<T> GetConverter<T>()
        {
            var converter = _convertersForNamespaces.Select(kvp => kvp.Value).OfType<IConverter<T>>().FirstOrDefault();
            if (converter == null)
            {
                throw new NotImplementedException(string.Format("No converter for {0} found ", typeof (T)));
            }
            return converter;
        }

        private Document PopDocument(IndexWriter writer, string sha1)
        {
            var searcher = new IndexSearcher(writer.GetDirectory(), false);
            try
            {
                var query = new TermQuery(new Term(SpecialFields.Sha1, sha1));
                var documents = searcher.Search(query, 1);

                Debug.Assert(documents.totalHits <= 1, string.Format("Sha1 '{0}' matched more than one document", sha1));

                if (documents.totalHits == 0) return null;

                var document = searcher.Doc(documents.scoreDocs.First().doc);
                writer.DeleteDocuments(new Term(SpecialFields.Sha1, sha1));
                return document;
            }
            finally
            {
                searcher.Close();
            }
        }

        public void SetConverters(IEnumerable<IConverter> converters)
        {
            _convertersForNamespaces = converters.ToDictionary(c => c.GetNamespaceForItems());
        }
    }
}