using System;
using System.Collections.Generic;
using System.Linq;

public static class LINQUtils
{
    public static IEnumerable<T> Traverse<T>(this T source, Func<T, IEnumerable<T>> childSelector)
    {
        var stack = new Queue<T>();
        stack.Enqueue(source);
        while (stack.Any())
        {
            var next = stack.Dequeue();
            yield return next;
            var childs = childSelector(next);
            if (childs == null) continue;
            foreach (var child in childs)
                stack.Enqueue(child);
        }
    }
}