using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Version = Lucene.Net.Util.Version;

namespace Core.Lucene
{
    public class AutoCompleteBasedOnLucene : IAutoCompleteText
    {
        private readonly SourceStorageFactory _sourceStorageFactory;
        private readonly ILog _log;
        private readonly IDirectoryFactory _directoryFactory;
        private readonly LuceneStorage _storage;

        [ImportMany]
        public IEnumerable<IConverter> Converters { get; set; }

        [Import(AllowRecomposition = true)]
        public AutoCompleteConfiguration Configuration { get; set; }

        public AutoCompleteBasedOnLucene(IDirectoryFactory directoryFactory, LuceneStorage luceneStorage, SourceStorageFactory sourceStorageFactory, ILog log)
        {
            _sourceStorageFactory = sourceStorageFactory;
            _log = log;
            _directoryFactory = directoryFactory;
            _storage = luceneStorage;
        }

        public AutoCompletionResult Autocomplete(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return AutoCompletionResult.NoResult(text);

            var searchers = _directoryFactory.GetAllDirectories().Select(d => new IndexSearcher(d, true)).ToArray();
            var searcher = new MultiSearcher(searchers);
            try
            {
                BooleanQuery query = GetQueryForText(text);

                var results = searcher.Search(query, 10);
                var commands = results.scoreDocs
                    .Select(d =>
                                {
                                    var document = searcher.Doc(d.doc);
                                    try
                                    {
                                        return _storage.GetCommandResultForDocument(document);
                                    }
                                    catch (Exception e)
                                    {
                                        _log.Error(e, "Error getting command result for document {0}:{1}",
                                                   document.GetField(SpecialFields.ConverterId).StringValue(),
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
            finally
            {
                searcher.Close();
            }
        }

        public void LearnInputForCommandResult(string input, AutoCompletionResult.CommandResult result)
        {
            var storage = _sourceStorageFactory.SourceStorageFor(result.CompletionId.SourceId);

            storage.LearnCommandForInput(result.CompletionId, input);
        }

        private BooleanQuery GetQueryForText(string text)
        {
            var queryParser = new MultiFieldQueryParser(Version.LUCENE_29,
                                                        new[] { SpecialFields.Name, SpecialFields.Learnings, SpecialFields.Type },
                                                        new StandardAnalyzer(Version.LUCENE_29));
            queryParser.SetFuzzyMinSim((float)Configuration.FuzzySimilarity);
            queryParser.SetDefaultOperator(QueryParser.Operator.AND);

            var textWithSubString = "*" + text.Trim().Replace(" ", "* *").Trim() + "*";
            var textWithFuzzy = text.Trim().Replace(" ", "~ ").Trim() + "~";

            queryParser.SetAllowLeadingWildcard(true);

            Query substringQuery = queryParser.Parse(textWithSubString);
            Query fuzzyQuery = queryParser.Parse(textWithFuzzy);

            var query = new BooleanQuery();
            query.Add(fuzzyQuery, BooleanClause.Occur.SHOULD);
            query.Add(substringQuery, BooleanClause.Occur.SHOULD);
            return query;
        }
    }
}