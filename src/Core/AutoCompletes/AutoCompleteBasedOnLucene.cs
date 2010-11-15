using System;
using System.Collections;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using Core.Abstractions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Core.AutoCompletes
{
    [Export(typeof(IAutoCompleteText))]
    public class AutoCompleteBasedOnLucene : IAutoCompleteText
    {
        private ShortcutFinder _finder;
        private Directory _directory;

        private Hashtable _stopwords;
        public AutoCompleteBasedOnLucene()
        {
            _stopwords = new Hashtable(ShortcutFinder._extensions.ToDictionary(s => s, s => s));
            
            var indexDirectory = new DirectoryInfo(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().FullName).DirectoryName, "index"));
            var createIndex = !indexDirectory.Exists;
            _directory = new SimpleFSDirectory(indexDirectory);
            new IndexWriter(_directory, new StandardAnalyzer(Version.LUCENE_29), createIndex, IndexWriter.MaxFieldLength.UNLIMITED).Close();

            _finder = new ShortcutFinder(files =>
                                             {
                                                 if(files.Count() == 0) return;

                                                 var indexWriter = new IndexWriter(_directory, new StandardAnalyzer(Version.LUCENE_29, _stopwords), mfl: IndexWriter.MaxFieldLength.UNLIMITED);
                                                 try
                                                 {
                                                     foreach (var fileInfo in files)
                                                     {
                                                         indexWriter.DeleteDocuments(new Term("filepath", fileInfo.FullName));

                                                         var name = new Field("filename", Path.GetFileNameWithoutExtension(fileInfo.Name), Field.Store.YES,
                                                                              Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                                                         var path = new Field("filepath", fileInfo.FullName, Field.Store.YES,
                                                                              Field.Index.NOT_ANALYZED_NO_NORMS);
                                                         var document = new Document();
                                                         document.Add(name);
                                                         document.Add(path);
                                                         indexWriter.AddDocument(document);
                                                     }
                                                     indexWriter.Commit();
                                                 }
                                                 finally
                                                 {
                                                     indexWriter.Close();
                                                 }
                                             });
        }

        public AutoCompletionResult Autocomplete(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return AutoCompletionResult.NoResult(text);

            var searcher = new IndexSearcher(_directory, true);
            try
            {
                QueryParser queryParser = new QueryParser(Version.LUCENE_29, "filename",
                                                          new StandardAnalyzer(Version.LUCENE_29, _stopwords));
                queryParser.SetDefaultOperator(QueryParser.Operator.AND);
                var results = searcher.Search(queryParser.Parse(text), 10);
                var commands = results.scoreDocs
                    .Select(d => searcher.Doc(d.doc).GetField("filepath").StringValue())
                    .Select(path => new FileInfoCommand(new FileInfo(path)));
                return AutoCompletionResult.OrderedResult(text, commands);
            }
            catch (ParseException e)
            {
                return AutoCompletionResult.SingleResult(text, new TextCommand(text, "Error parsing input: " + e.Message));
            }
            
        }
    }
}