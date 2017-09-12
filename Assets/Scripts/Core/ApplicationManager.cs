using System;
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

        private const string TreeDataFile = "AirToolsStructure.json";

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

        private static Vector2 CalcTreePosition(IReadOnlyCollection<UiNode> trees, int n)
        {
            var rowDistance = trees.Select(tree => tree.GetWidth()).Max() * 0.0025f;
            var forestLength = (int) Math.Floor(Math.Sqrt(trees.Count));
            var x = (n % forestLength - forestLength / 2) * rowDistance;
            var y = (n / forestLength - forestLength / 2) * rowDistance;
            return new Vector2(x, y);
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