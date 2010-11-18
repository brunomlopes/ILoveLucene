using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Directory = Lucene.Net.Store.Directory;

namespace Core.Lucene
{
    [Export(typeof(IAutoCompleteText))]
    public class AutoCompleteBasedOnLucene : IAutoCompleteText
    {
        private readonly IEnumerable<IItemSource> _sources;
        private Directory _directory;

        private IEnumerable<IConverter> _converters;

        [ImportingConstructor]
        public AutoCompleteBasedOnLucene([ImportMany]IEnumerable<IConverter> converters, [ImportMany]IEnumerable<IItemSource> sources)
        {
            _sources = sources;
            _converters = converters;
            EnsureIndexExists();

            var host = new LuceneStorage(_converters);

            _sources
                .AsParallel()
                .ForAll(s => s.GetItems()
                                 .ContinueWith(task =>
                                                   {
                                                       var items = task.Result;
                                                       if (items.Count() == 0) return;

                                                       IndexItems(items, host);
                                                   }));
        }

        private void IndexItems(IEnumerable<object> items, LuceneStorage host)
        {
            var indexWriter = GetIndexWriter();
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

        private IndexWriter GetIndexWriter()
        {
            return new IndexWriter(_directory, new StandardAnalyzer(Version.LUCENE_29),
                                   IndexWriter.MaxFieldLength.UNLIMITED);
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
                var textWithFuzzy = text.Trim().Replace(" ", "~ ").Trim() + "~";
                var queryParser = new MultiFieldQueryParser(Version.LUCENE_29, new string[]{SpecialFields.Name, SpecialFields.Learnings},
                                                          new StandardAnalyzer(Version.LUCENE_29));
                var converterHost = new LuceneStorage(_converters);
                queryParser.SetFuzzyMinSim((float)0.2);
                queryParser.SetDefaultOperator(QueryParser.Operator.AND);

                var results = searcher.Search(queryParser.Parse(textWithFuzzy), 10);
                var commands = results.scoreDocs
                    .Select(d => converterHost.GetCommandResultForDocument(searcher.Doc(d.doc)));
                return AutoCompletionResult.OrderedResult(text, commands);
            }
            catch (ParseException e)
            {
                return AutoCompletionResult.SingleResult(text,
                                                         new AutoCompletionResult.CommandResult(
                                                             new TextCommand(text, "Error parsing input: " + e.Message),
                                                             null));
            }
            
        }

        public void LearnInputForCommandResult(string input, AutoCompletionResult.CommandResult result)
        {
            var writer = GetIndexWriter();

            try
            {
                var host = new LuceneStorage(_converters);
                host.LearnCommandForInput(writer, result.CompletionId, input);
            }
            finally
            {
                writer.Close();
            }
        }
    }
}