using System.IO;
using HoloToolkit.Unity;
using Newtonsoft.Json;
using UnityEngine;

namespace Data
{
    public class StreamingAssetsService : Singleton<StreamingAssetsService>
    {      
        public void SerializeData(object obj, string fileName)
        {
            fileName = fileName + ".json";
            var path = Path.Combine(Application.streamingAssetsPath, fileName);
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public T DesirializeData<T>(string fileName)
        {
            fileName = fileName + ".json";
            var path = Path.Combine(Application.streamingAssetsPath, fileName);
            if (!File.Exists(path))
            {
                Debug.LogError("Connot read data, for there is no such file.");
            }
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}