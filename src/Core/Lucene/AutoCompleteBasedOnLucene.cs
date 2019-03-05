using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.API;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;

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
                                                                                     
                                                                                     return (IndexReader)DirectoryReader.Open(d);
                                                                                 }
                                                                                 catch (Exception e)
                                                                                 {
                                                                                     _log.Error(e, "While searching directory {0}", d);
                                                                                     return null;
                                                                                 }
                                                                             })
                                                                             .Where(s => s != null)
                                                                             .ToArray();
            using(var multiReader = new MultiReader(searchers, true))
            {
                var searcher = new IndexSearcher(multiReader);
                try
                {
                    BooleanQuery query = GetQueryForText(text);
                
                    var results = searcher.Search(query, 10);
                    var commands = results.ScoreDocs
                        .Select(d =>
                                    {
                                        var document = searcher.Doc(d.Doc);
                                        try
                                        {
                                            Explanation explanation = null;
                                            if (includeExplanation)
                                            {
                                                explanation = searcher.Explain(query, d.Doc);
                                            }
                                            var coreDoc = CoreDocument.Rehydrate(document);
                                            var command = _converterRepository.FromDocumentToItem(coreDoc);

                                            return new AutoCompletionResult.CommandResult(command, coreDoc.GetDocumentId(), explanation);
                                        }
                                        catch (Exception e)
                                        {
                                            _log.Error(e, "Error getting command result for document {0}:{1}",
                                                       document.GetField(SpecialFields.ConverterId).GetStringValue(),
                                                       document.GetField(SpecialFields.Id).GetStringValue());
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
            var queryParser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48,
                                                        new[] { SpecialFields.Name, SpecialFields.Learnings, SpecialFields.Type },
                                                        new StandardAnalyzer(LuceneVersion.LUCENE_48)
                                                        //,boosts
                                                        );
            
            queryParser.FuzzyMinSim = ((float)Configuration.FuzzySimilarity);
            queryParser.DefaultOperator = (QueryParserBase.AND_OPERATOR);

            var textWithSubString = "*" + text.Trim().Replace(" ", "* *").Trim() + "*";
            var textWithFuzzy = text.Trim().Replace(" ", "~ ").Trim() + "~";

            queryParser.AllowLeadingWildcard = (true);

            Query substringQuery = queryParser.Parse(textWithSubString);
            Query fuzzyQuery = queryParser.Parse(textWithFuzzy);

            var query = new BooleanQuery {{fuzzyQuery, Occur.SHOULD}, {substringQuery, Occur.SHOULD}};

            return query;
        }
    }
}