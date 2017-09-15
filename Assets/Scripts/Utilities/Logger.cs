using Newtonsoft.Json;
using UnityEngine;

namespace Utilities
{
    public static class Logger
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        
        public static void Log(object obj)
        {
            Debug.Log(JsonConvert.SerializeObject(obj, Formatting.Indented, JsonSerializerSettings));
        }
    
        public static string Json(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, JsonSerializerSettings);
        }
    }
}