using System.Collections.Generic;
using Core.API;
using Core.Abstractions;
using JetBrains.Annotations;

namespace ILoveLucene.Infrastructure
{
    public static class ListWithCurrentSelectionExtensions
    {
        [NotNull]
        public static ListWithCurrentSelection<T> ToListWithCurrentSelection<T>(this IEnumerable<T> self)
        {
            return new ListWithCurrentSelection<T>(self);
        }

        [NotNull]
        public static ListWithCurrentSelection<AutoCompletionResult.CommandResult> ToListWithCurrentSelection(this IItem self, DocumentId documentId = null)
        {
            return new ListWithCurrentSelection<AutoCompletionResult.CommandResult>(
                new AutoCompletionResult.CommandResult(self, documentId));
            
        }
    }
}