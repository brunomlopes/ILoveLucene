using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Core.Lucene
{
    public class AutoCompleteBasedOnLucene : LuceneBase, IAutoCompleteText
    {
        private readonly CompositionContainer _mefContainer;

        [ImportMany]
        public IEnumerable<IConverter> Converters { get; set; }

        public AutoCompleteBasedOnLucene(CompositionContainer mefContainer)
        {
            _mefContainer = mefContainer;
            _mefContainer.SatisfyImportsOnce(this);
        }

        private AutoCompleteBasedOnLucene(Directory directory)
            : base(directory)
        {
            
        }

        public static AutoCompleteBasedOnLucene WithDirectory(Directory directory)
        {
            return new AutoCompleteBasedOnLucene(directory);
        }

        public AutoCompletionResult Autocomplete(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return AutoCompletionResult.NoResult(text);

            var searcher = new IndexSearcher(Directory, true);
            try
            {
                var textWithFuzzy = text.Trim().Replace(" ", "~ ").Trim() + "~";
                var queryParser = new MultiFieldQueryParser(Version.LUCENE_29,
                                                            new[] {SpecialFields.Name, SpecialFields.Learnings},
                                                            new StandardAnalyzer(Version.LUCENE_29));
                var converterHost = new LuceneStorage(Converters);
                queryParser.SetFuzzyMinSim((float) 0.2);
                queryParser.SetDefaultOperator(QueryParser.Operator.AND);

                var results = searcher.Search(queryParser.Parse(textWithFuzzy), 10);
                var commands = results.scoreDocs
                    .Select(d =>
                                {
                                    try
                                    {
                                        return converterHost.GetCommandResultForDocument(searcher.Doc(d.doc));
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine("Error getting command result for document:" + e);
                                        return null;
                                    }
                                })
                    .Where(r => r != null);
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
                var host = new LuceneStorage(Converters);
                host.LearnCommandForInput(writer, result.CompletionId, input);
            }
            finally
            {
                writer.Close();
            }
        }
    }
}