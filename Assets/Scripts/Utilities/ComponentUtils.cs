using UnityEngine;

public static class ComponentUtils
{
    public static void AddComponent<T>(this GameObject obj, object parameter) where T : Component
    {
        var fields = typeof(T).GetFields();
        if (fields.Length == 0 || fields.Length > 1)
        {
            Debug.LogError("Error at initalization of component field.");
            return;
        }
        var component = obj.AddComponent<T>();
        fields[0].SetValue(component, parameter);
    }
}