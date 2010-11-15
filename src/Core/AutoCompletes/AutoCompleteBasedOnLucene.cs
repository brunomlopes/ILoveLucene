using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using Core.Abstractions;
using Core.Converters;
using Lucene.Net.Analysis.Standard;
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
        private Directory _directory;

        public IEnumerable<IConverter> Converters { get; set; }

        [ImportingConstructor]
        public AutoCompleteBasedOnLucene([ImportMany]IEnumerable<IConverter> converters, [ImportMany]IEnumerable<IItemSource> sources)
        {
            Converters = converters;
            EnsureIndexExists();

            var host = new ConverterHost(Converters);

            sources
                .AsParallel()
                .ForAll(s => s.GetItems()
                                 .ContinueWith(task =>
                                                   {
                                                       var files = task.Result;
                                                       if (files.Count() == 0) return;

                                                       IndexItems(files, host);
                                                   }));
        }

        private void IndexItems(IEnumerable<object> items, ConverterHost host)
        {
            var indexWriter = new IndexWriter(_directory, new StandardAnalyzer(Version.LUCENE_29),
                                              IndexWriter.MaxFieldLength.UNLIMITED);
            try
            {
                foreach (var item in items)
                {
                    host.UpdateDocumentForObject(indexWriter,item);
                }
                indexWriter.Commit();
            }
            finally
            {
                indexWriter.Close();
            }
        }

        private void EnsureIndexExists()
        {
            var indexDirectory = new DirectoryInfo(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().FullName).DirectoryName, "index"));
            var createIndex = !indexDirectory.Exists;
            _directory = new SimpleFSDirectory(indexDirectory);
            new IndexWriter(_directory, new StandardAnalyzer(Version.LUCENE_29), createIndex, IndexWriter.MaxFieldLength.UNLIMITED).Close();
        }

        public AutoCompletionResult Autocomplete(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return AutoCompletionResult.NoResult(text);

            var searcher = new IndexSearcher(_directory, true);
            try
            {
                QueryParser queryParser = new QueryParser(Version.LUCENE_29, "_name",
                                                          new StandardAnalyzer(Version.LUCENE_29));
                var converterHost = new ConverterHost(Converters);

                queryParser.SetDefaultOperator(QueryParser.Operator.AND);
                var results = searcher.Search(queryParser.Parse(text), 10);
                var commands = results.scoreDocs
                    .Select(d => converterHost.GetCommandForDocument(searcher.Doc(d.doc)));
                return AutoCompletionResult.OrderedResult(text, commands);
            }
            catch (ParseException e)
            {
                return AutoCompletionResult.SingleResult(text, new TextCommand(text, "Error parsing input: " + e.Message));
            }
            
        }
    }
}