using System.Collections.Generic;

namespace ILoveLucene.Infrastructure
{
    public static class ListWithCurrentSelectionExtensions
    {
        public static ListWithCurrentSelection<T> ToListWithCurrentSelection<T>(this IEnumerable<T> self)
        {
            return new ListWithCurrentSelection<T>(self);
        }
    }
}