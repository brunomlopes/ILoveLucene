using System.Collections.Generic;
using System.Linq;

namespace Core.API
{
    public class ArgumentAutoCompletionResult
    {
        public bool HasAutoCompletion
        {
            get { return AutoCompletedArgument != null; }
        }

        public string OriginalText { get; private set; }
        public string AutoCompletedArgument { get; private set; }
        public IEnumerable<string> OtherOptions { get; private set; }

        protected ArgumentAutoCompletionResult()
        {
            OtherOptions = new string[] {};
        }

        public static ArgumentAutoCompletionResult NoResult(string originalText)
        {
            return new ArgumentAutoCompletionResult {OriginalText = originalText, AutoCompletedArgument = null};
        }

        public static ArgumentAutoCompletionResult OrderedResult(string originalText, IEnumerable<string> result)
        {
            result = result.ToList();
            if (result.Count() == 0)
            {
                return new ArgumentAutoCompletionResult {OriginalText = originalText, AutoCompletedArgument = null};
            }
            return new ArgumentAutoCompletionResult
                       {
                           OriginalText = originalText,
                           AutoCompletedArgument = result.First(),
                           OtherOptions = result.Skip(1)
                       };
        }

        public static ArgumentAutoCompletionResult SingleResult(string text, string textCommand)
        {
            return OrderedResult(text, new[] {textCommand});
        }
    }
}