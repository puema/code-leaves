using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    public static class LinkedListExtensions
    {
        public static LinkedListNode<T> Next<T>(this LinkedListNode<T> current)
        {
            return current.Next ?? current.List.First;
        }

        public static LinkedListNode<T> Previous<T>(this LinkedListNode<T> current)
        {
            return current.Previous ?? current.List.Last;
        }
        
        public static int GetIndex<T>(this LinkedList<T> list, T node)
        {
            var index = list.TakeWhile(n => n.Equals(node)).Count();
            if (index == list.Count)
                throw new InvalidOperationException("No match.");
            return index == list.Count ? -1 : index;
        }
    }
}