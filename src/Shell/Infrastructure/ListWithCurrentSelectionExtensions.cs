using System.Collections.Generic;
using Core.Abstractions;

namespace ILoveLucene.Infrastructure
{
    public static class ListWithCurrentSelectionExtensions
    {
        public static ListWithCurrentSelection<T> ToListWithCurrentSelection<T>(this IEnumerable<T> self)
        {
            return new ListWithCurrentSelection<T>(self);
        }

        public static ListWithCurrentSelection<AutoCompletionResult.CommandResult> ToListWithCurrentSelection(this IItem self, DocumentId documentId = null)
        {
            return new ListWithCurrentSelection<AutoCompletionResult.CommandResult>(
                new AutoCompletionResult.CommandResult(self, documentId));
            
        }
    }
}