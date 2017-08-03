using Newtonsoft.Json;
using UnityEngine;

public class Logger
{
    public static void Log(object obj)
    {
        Debug.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
    }
    
    public static string Json(object obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }
}