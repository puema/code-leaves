using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    public static class LINQUtils
    {
        public static IEnumerable<T> Traverse<T>(this T source, Func<T, IEnumerable<T>> childSelector)
        {
            var queue = new Queue<T>();
            queue.Enqueue(source);
            while (queue.Any())
            {
                var next = queue.Dequeue();
                yield return next;
                var childs = childSelector(next);
                if (childs == null) continue;
                foreach (var child in childs)
                    queue.Enqueue(child);
            }
        }
    }
}