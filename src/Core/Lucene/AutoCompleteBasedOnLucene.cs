﻿using System;
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
        [Import(AllowRecomposition = true)]
        public AutoCompleteConfiguration Configuration { get; set; }

        public AutoCompleteBasedOnLucene(IDirectoryFactory directoryFactory, LuceneStorage learningStorage)
            : base(directoryFactory, learningStorage)
        {}

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
                                    try
                                    {
                                        return Storage.GetCommandResultForDocument(searcher.Doc(d.doc));
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
                Debug.WriteLine("Error parsing: "+e);
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