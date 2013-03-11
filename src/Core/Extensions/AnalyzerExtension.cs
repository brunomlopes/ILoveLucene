using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;

namespace Core.Extensions
{
    public static class AnalyzerExtension
    {
        public static IEnumerable<string> Tokenize(this Analyzer self, string text)
        {
            using (var stream = self.TokenStream("ignored field name", new StringReader(text)))
            {
                stream.Reset();
                var termAtt = stream.AddAttribute<ITermAttribute>();

                while (stream.IncrementToken())
                {
                    yield return termAtt.Term;
                }
                stream.End();
            }
        }
    }
}