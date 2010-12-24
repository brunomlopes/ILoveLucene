using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Core.Lucene
{
    public class AutoCompleteBasedOnLucene : LuceneBase, IAutoCompleteText
    {
        private readonly ILog _log;

        [Import(AllowRecomposition = true)]
        public AutoCompleteConfiguration Configuration { get; set; }

        public AutoCompleteBasedOnLucene(IDirectoryFactory directoryFactory, LuceneStorage learningStorage, ILog log)
            : base(directoryFactory, learningStorage)
        {
            _log = log;
        }

        public AutoCompletionResult Autocomplete(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return AutoCompletionResult.NoResult(text);

            var searchers = DirectoryFactory.GetAllDirectories().Select(d => new IndexSearcher(d, true)).ToArray();
            var searcher = new MultiSearcher(searchers);
            try
            {
                var queryParser = new MultiFieldQueryParser(Version.LUCENE_29,
                                                            new[] {SpecialFields.Name, SpecialFields.Learnings},
                                                            new StandardAnalyzer(Version.LUCENE_29));
                queryParser.SetFuzzyMinSim((float)Configuration.FuzzySimilarity);
                queryParser.SetDefaultOperator(QueryParser.Operator.AND);

                var textWithSubString = "*"+text.Trim().Replace(" ", "* *").Trim() + "*";
                var textWithFuzzy = text.Trim().Replace(" ", "~ ").Trim() + "~";

                queryParser.SetAllowLeadingWildcard(true);

                Query substringQuery = queryParser.Parse(textWithSubString);
                Query fuzzyQuery = queryParser.Parse(textWithFuzzy);

                
                var query = new BooleanQuery();
                query.Add(fuzzyQuery, BooleanClause.Occur.SHOULD);
                query.Add(substringQuery, BooleanClause.Occur.SHOULD);

                var results = searcher.Search(query, 10);
                var commands = results.scoreDocs
                    .Select(d =>
                                {
                                    var document = searcher.Doc(d.doc);
                                    try
                                    {
                                        return Storage.GetCommandResultForDocument(document);
                                    }
                                    catch (Exception e)
                                    {
                                        _log.Error(e, "Error getting command result for document {0}:{1}",
                                                   document.GetField(SpecialFields.Namespace).StringValue(),
                                                   document.GetField(SpecialFields.Id).StringValue());
                                        return null;
                                    }
                                })
                    .Where(r => r != null);
                return AutoCompletionResult.OrderedResult(text, commands);
            }
            catch (ParseException e)
            {
                _log.Error(e, "Error parsing '{0}'", text);
                return AutoCompletionResult.NoResult(text);
            }
        }

        public void LearnInputForCommandResult(string input, AutoCompletionResult.CommandResult result)
        {
            IndexWriter writer = null;
            try
            {
                writer = GetIndexWriter();
                Storage.LearnCommandForInput(writer, result.CompletionId, input);
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        }
    }
}