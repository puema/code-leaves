using Newtonsoft.Json;
using UnityEngine;

public class Logger
{
    public static void Json(object obj)
    {
        Debug.Log(JsonConvert.SerializeObject(obj, Formatting.Indented));
    }
}