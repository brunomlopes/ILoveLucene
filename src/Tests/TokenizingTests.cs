using System;
using System.Collections.Generic;
using System.Linq;
using Core.Lucene;
using Lucene.Net.Analysis.Standard;
using Xunit;
using Version = Lucene.Net.Util.Version;

namespace Tests
{
   
    public class TokenizingTests
    {
        [Fact]
        public void ExtractTokensFromString()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);

            var tokens = analyzer.Tokenize("notepad2 note2 note2 no2 n2 n2 n2 n2 n2 n2 note2 notepad2");
            
            Assert.Equal(12, tokens.Count());

            var deduplicatedTokens = new HashSet<string>(tokens);
            Assert.Equal(4, deduplicatedTokens.Count);
            Assert.Contains("notepad2", deduplicatedTokens);
            Assert.Contains("note2", deduplicatedTokens);
            Assert.Contains("no2", deduplicatedTokens);
            Assert.Contains("n2", deduplicatedTokens);
        }
        
    }
}