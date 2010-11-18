using System;
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
                var textWithFuzzy = text.Trim().Replace(" ", "~ ").Trim()+"~";
                var queryParser = new MultiFieldQueryParser(Version.LUCENE_29, new string[]{"_name", "_learnings"},
                                                          new StandardAnalyzer(Version.LUCENE_29));
                var converterHost = new ConverterHost(Converters);
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
                var host = new ConverterHost(Converters);
                host.LearnCommandForInput(writer, result.CompletionId, input);
            }
            finally
            {
                writer.Close();
            }
        }
    }
}