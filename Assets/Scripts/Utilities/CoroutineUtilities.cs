using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoroutineUtils
{
    public static Coroutine<T> StartCoroutine<T>(this MonoBehaviour obj, IEnumerator coroutine)
    {
        return new Coroutine<T>(obj, coroutine);
    }
}

public class Coroutine<T>
{
    public T value;
    public Coroutine coroutine;
    
    public Coroutine(MonoBehaviour owner, IEnumerator target) {
        coroutine = owner.StartCoroutine(InernalRoutine(target));
    }

    public IEnumerator InernalRoutine(IEnumerator enumerator)
    {
        while(enumerator.MoveNext()) {
            var current = enumerator.Current;
            if (current is T)
            {
                value = (T) current;
            }
            else
            {
                // Forward current to unity's coroutine
                yield return current;
            }
        }
    }
}

public static class EnumeratorUtils
{
    public static IEnumerator WaitForAll(List<IEnumerator> enumerators)
    {
        while (enumerators.Exists(x => x.MoveNext()))
        {
            yield return null;
        }
    }
}