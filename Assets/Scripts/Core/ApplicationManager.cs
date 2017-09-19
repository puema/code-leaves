using System;
using System.Collections;
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
        // ----==== Dependencies ====---- //
        public SonarQubeService SonarQubeService;
        // ------------------------------ //
        
        public Forest Forest;
        public AppState AppState;

        private readonly AppState InitialAppState = new AppState
        {
            FloorInteractionMode = new ReactiveProperty<FloorInteractionMode>(FloorInteractionMode.TapToMenu),
            AppData = null
        };

        private const string TreeDataFile = "AirStructure2.json";

        private readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        protected override void Awake()
        {
            AppState = InitialAppState;

            var path = Path.Combine(Application.streamingAssetsPath, TreeDataFile);

            var softwareRoot = DesirializeData<Package>(path);

//            var coroutine = StartCoroutine(SonarQubeService.AddMetrics(softwareRoot));

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
            var forest = Forest.Root as UiInnerNode;
            forest?.SortChildren();
            var trees = forest?.Children ?? new List<UiNode>();
            for (var i = 0; i < trees.Count; i++)
            {
                var treeObject = TreeBuilder.Instance.GenerateTree(trees[i], CalcTreePosition(trees, i));
                trees[i].Circle.Position.Subscribe(v =>
                {
                    treeObject.transform.localPosition = new Vector3(v.x, 0, v.y);
                });
            }
            TreeBuilder.Instance.CirclePacking(Forest.Root as UiInnerNode);
            SceneManipulator.Instance.AdjustFloorRadius(Forest.Root);
        }

        private static Vector2 CalcTreePosition(IReadOnlyCollection<UiNode> trees, int n)
        {
            const float rowDistance = 1.3f;
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