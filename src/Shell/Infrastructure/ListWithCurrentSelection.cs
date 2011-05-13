using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ILoveLucene.Infrastructure
{
    public class ListWithCurrentSelection<T> : IEnumerable<T>
    {
        private readonly T[] _items;
        private int currentIndex;

        public ListWithCurrentSelection(IEnumerable<T> items)
        {
            _items = items.ToArray();
            currentIndex = 0;
        }
        
        public ListWithCurrentSelection(params T[] items)
        {
            _items = items.ToArray();
            currentIndex = 0;
        }

        public T Current
        {
            get
            {
                if (_items.Length == 0)
                    return default(T);
                return _items[currentIndex];
            }
        }

        public IEnumerable<T> Items
        {
            get { return _items; }
        }

        public T SetIndex(int i)
        {
            currentIndex = i;
            return Current;
        }

        public T Next()
        {
            currentIndex += 1;
            currentIndex = currentIndex%_items.Length;
            return Current;
        }

        public T Previous()
        {
            currentIndex += _items.Length - 1;
            currentIndex = currentIndex%_items.Length;
            return Current;
        }

        public int Count
        {
            get { return _items.Length; }
        }


        public IEnumerator<T> GetEnumerator()
        {
            return _items.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}