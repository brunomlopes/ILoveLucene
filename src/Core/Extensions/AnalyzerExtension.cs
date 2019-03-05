using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;

namespace Core.Extensions
{
    public static class AnalyzerExtension
    {
        public static IEnumerable<string> Tokenize(this Analyzer self, string text)
        {
            using (var stream = self.GetTokenStream("ignored field name", new StringReader(text)))
            {
                // this was taken from https://stackoverflow.com/questions/42180061/how-to-get-an-analyzed-term-from-a-tokenstream-in-lucene-net-4-8
                stream.Reset();
                var termAtt = stream.AddAttribute<ICharTermAttribute>();

                while (stream.IncrementToken())
                {
                    yield return termAtt.ToString();
                }
                stream.End();
            }
        }
    }
}