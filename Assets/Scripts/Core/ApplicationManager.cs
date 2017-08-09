using System.IO;
using Core;
using HoloToolkit.Unity;
using Newtonsoft.Json;
using UnityEngine;

public class ApplicationManager : Singleton<ApplicationManager>
{
    public UiNode Root;
    
    private const string TreeDataFile = "TreeStructureTypes.json";
    private readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };
    private string TreeDataPath;

    private void Start()
    {
        TreeDataPath = Path.Combine(Application.streamingAssetsPath, TreeDataFile);
        Root = DesirializeData();
        InteractionManager.Instance.Root = Root;

//        SerializeData(Root);

        TreeBuilder.Instance.GenerateTreeStructure((UiInnerNode) Root);
    }
    
    private void SerializeData(object obj)
    {
        var json = JsonConvert.SerializeObject(obj, Formatting.Indented, JsonSerializerSettings);
        File.WriteAllText(TreeDataPath, json);
    }

    private UiInnerNode DesirializeData()
    {
        if (!File.Exists(TreeDataPath))
        {
            Debug.LogError("Connot load tree data, for there is no such file.");
            return null;
        }
        var json = File.ReadAllText(TreeDataPath);
        return JsonConvert.DeserializeObject<UiInnerNode>(json, JsonSerializerSettings);
    }
}