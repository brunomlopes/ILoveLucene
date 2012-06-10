using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.API;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Version = Lucene.Net.Util.Version;

namespace Core.Lucene
{
    public class AutoCompleteBasedOnLucene
    {
        private readonly SourceStorageFactory _sourceStorageFactory;
        private readonly ILog _log;
        private readonly IConverterRepository _converterRepository;
        private readonly IDirectoryFactory _directoryFactory;

        [ImportMany]
        public IEnumerable<IConverter> Converters { get; set; }

        [ImportConfiguration]
        public AutoCompleteConfiguration Configuration { get; set; }

        public AutoCompleteBasedOnLucene(IDirectoryFactory directoryFactory, SourceStorageFactory sourceStorageFactory, ILog log, IConverterRepository converterRepository)
        {
            _sourceStorageFactory = sourceStorageFactory;
            _log = log;
            _converterRepository = converterRepository;
            _directoryFactory = directoryFactory;
        }

        public AutoCompletionResult Autocomplete(string text, bool includeExplanation = false)
        {
            if (string.IsNullOrWhiteSpace(text)) return AutoCompletionResult.NoResult(text);

            var searchers = _directoryFactory.GetAllDirectories().Select(d =>
                                                                             {
                                                                                 try
                                                                                 {
                                                                                     return new IndexSearcher(d, true);
                                                                                 }
                                                                                 catch (Exception e)
                                                                                 {
                                                                                     _log.Error(e, "While searching directory {0}", d);
                                                                                     return null;
                                                                                 }
                                                                             })
                                                                             .Where(s => s != null)
                                                                             .ToArray();
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
                                        Explanation explanation = null;
                                        if (includeExplanation)
                                        {
                                            explanation = searcher.Explain(query, d.doc);
                                        }
                                        var coreDoc = CoreDocument.Rehydrate(document);
                                        var command = _converterRepository.FromDocumentToItem(coreDoc);

                                        return new AutoCompletionResult.CommandResult(command, coreDoc.GetDocumentId(), explanation);
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
            if (result.IsTransient()) return;

            var storage = _sourceStorageFactory.SourceStorageFor(result.CompletionId.SourceId);

            storage.LearnCommandForInput(result.CompletionId, input);
        }

        private BooleanQuery GetQueryForText(string text)
        {
            // boosts are not working as I think they would
            var boosts = new Dictionary<string, float>()
                             {
                                 {SpecialFields.Learnings, 10},
                                 {SpecialFields.Type, 4}
                             };
            var queryParser = new MultiFieldQueryParser(Version.LUCENE_29,
                                                        new[] { SpecialFields.Name, SpecialFields.Learnings, SpecialFields.Type },
                                                        new StandardAnalyzer(Version.LUCENE_29)
                                                        //,boosts
                                                        );
            
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