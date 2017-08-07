using System.IO;
using HoloToolkit.Unity;
using Newtonsoft.Json;
using UnityEngine;

public class ApplicationManager : Singleton<ApplicationManager>
{
    public UiNode Root;
    
    private const string TreeDataFilePath = "Assets/StreamingAssets/TreeStructureTypes.json";
    private readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    private void Start()
    {
        Root = DesirializeData();
        InteractionManager.Instance.Root = Root;

//        SerializeData(Root);

        TreeBuilder.Instance.GenerateTreeStructure((UiInnerNode) Root);
    }
    
    private void SerializeData(object obj)
    {
        var json = JsonConvert.SerializeObject(obj, Formatting.Indented, JsonSerializerSettings);
        File.WriteAllText(TreeDataFilePath, json);
    }

    private UiInnerNode DesirializeData()
    {
        if (!File.Exists(TreeDataFilePath))
        {
            Debug.LogError("Connot load tree data, for there is no such file.");
            return null;
        }
        var json = File.ReadAllText(TreeDataFilePath);
        return JsonConvert.DeserializeObject<UiInnerNode>(json, JsonSerializerSettings);
    }
}