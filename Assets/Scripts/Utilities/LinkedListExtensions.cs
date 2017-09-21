using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

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

        public static bool IsAfter<T>(this LinkedListNode<T> find, LinkedListNode<T> node)
        {
            var found = false;
            var next = node;
            var previous = node;

            while (!found)
            {
                next = next.Next();
                previous = previous.Previous();

                if (next == previous || next.Next() == previous)
                {
                    throw new InvalidOperationException("No match.");
                }

                if (next == find || previous == find)
                    found = true;
            }
            
            return next.Equals(find);
        }

        public static void DeleteAfterUntil<T>(this LinkedListNode<T> after, LinkedListNode<T> until)
        {
            while (after.Next() != until)
            {
                after.List.Remove(after.Next());
            }
        }
    }
}