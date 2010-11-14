using System.Collections.Generic;
using System.Linq;

namespace Core.Abstractions
{
    public class AutoCompletionResult
    {
        public bool HasAutoCompletion
        {
            get { return AutoCompletedText != null; }
        }

        public string OriginalText { get; private set; }
        public string AutoCompletedText { get; private set; }
        public IEnumerable<string> OtherOptions { get; private set; }

        protected AutoCompletionResult()
        {
            OtherOptions = new string[]{};
        }

        public static AutoCompletionResult NoResult(string originalText)
        {
            return new AutoCompletionResult(){OriginalText = originalText, AutoCompletedText = null};
        }
        
        public static AutoCompletionResult OrderedResult(string originalText, IEnumerable<string> result)
        {
            return new AutoCompletionResult(){OriginalText = originalText, AutoCompletedText = result.First(), OtherOptions = result.Skip(1)};
        }
    }
}