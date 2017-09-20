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

        public string ProjectName = "Dcom";

        public bool GetDataFromSonarQube;

        public SonarQubeService SonarQubeService;

        public string SonarQubeBaseComponent = "com.bmw.dcom:dcom";

        // ------------------------------ //

        public ReactiveProperty<Forest> Forest = new ReactiveProperty<Forest>();

        public AppState AppState = new AppState
        {
            FloorInteractionMode = new ReactiveProperty<FloorInteractionMode>(FloorInteractionMode.TapToMenu),
            AppData = null
        };

        private IEnumerator Start()
        {
            SoftwareArtefact softwareRoot = null;

            if (!GetDataFromSonarQube)
            {
                var fileName = ProjectName + ".json";
                var path = Path.Combine(Application.streamingAssetsPath, fileName);
                softwareRoot = DesirializeData<Package>(path);
            }

            if (GetDataFromSonarQube)
            {
                var sonarQubeCoroutine =
                    this.StartCoroutine<SoftwareArtefact>(SonarQubeService.GetSoftwareArtefact(
                        SonarQubeBaseComponent,
                        ProjectName));
                yield return sonarQubeCoroutine.coroutine;
                softwareRoot = sonarQubeCoroutine.value;
            }

            var node = SoftwareArtefactToNodeMapper.Map(softwareRoot);

            Forest.Value = new Forest
            {
                Root = AppToUiMapper.Map(node)
            };

            StartCoroutine(Render());
        }

        private IEnumerator Render()
        {
            var forest = Forest.Value.Root as UiInnerNode;
            forest?.SortChildren();
            var trees = forest?.Children ?? new List<UiNode>();
            for (var i = 0; i < trees.Count; i++)
            {
                var treeObject = TreeBuilder.Instance.GenerateTree(trees[i], CalcTreePosition(trees, i));
                trees[i].Circle.Position.Subscribe(v =>
                {
                    treeObject.transform.localPosition = new Vector3(v.x, 0, v.y);
                });
                yield return null;
            }
            TreeBuilder.Instance.CirclePacking(forest);
            SceneManipulator.Instance.AdjustFloorRadius(forest);
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
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private static T DesirializeData<T>(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("Connot load tree data, for there is no such file.");
            }
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}