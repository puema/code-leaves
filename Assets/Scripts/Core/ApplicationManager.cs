using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using Frontend;
using HoloToolkit.Unity;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

namespace Core
{
    public class ApplicationManager : Singleton<ApplicationManager>
    {
        public Forest Forest;
        public AppState AppState;

        private readonly AppState InitialAppState = new AppState
        {
            FloorInteractionMode = new ReactiveProperty<FloorInteractionMode>(FloorInteractionMode.TapToMenu),
            AppData = null
        };

        private const string TreeDataFile = "AirStructure.json";

        private readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        protected override void Awake()
        {
            AppState = InitialAppState;

            var path = Path.Combine(Application.streamingAssetsPath, TreeDataFile);

            var softwareRoot = DesirializeData<Package>(path);

            var node = SoftwareArtefactToNodeMapper.Map(softwareRoot);

            Forest = new Forest
            {
                Root = AppToUiMapper.Map(node)
            };

            base.Awake();

//        SerializeData(Root);
        }

        private void Start()
        {
            var trees = (Forest.Root as UiInnerNode)?.Children ?? new List<UiNode>();
            var count = trees.Count;
            for (var i = 0; i < count; i++)
            {
                TreeBuilder.Instance.GenerateTree(trees[i], CalcTreePosition(trees, i));
            }
        }

        private Vector2 CalcTreePosition(IReadOnlyList<UiNode> trees, int n)
        {
            var x = n * 20f;
            return new Vector2(x, 0);
        }

        private void SerializeData(object obj, string path)
        {
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented, JsonSerializerSettings);
            File.WriteAllText(path, json);
        }

        private T DesirializeData<T>(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("Connot load tree data, for there is no such file.");
            }
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json, JsonSerializerSettings);
        }
    }
}