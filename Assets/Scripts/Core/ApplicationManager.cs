using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Frontend;
using HoloToolkit.Unity;
using UniRx;
using UnityEngine;

namespace Core
{
    public class ApplicationManager : Singleton<ApplicationManager>
    {
        // ----==== Dependencies ====---- //

        public bool GetDataFromSonarQube;

        public SonarQubeService SonarQubeService;

        public string SonarQubeBaseComponent = "com.bmw.dcom:dcom";

        public BoolReactiveProperty VisualizeCircles = new BoolReactiveProperty(false);

        // ------------------------------ //

        public AppState AppState = new AppState
        {
            ContexMenu = new ContextMenu
            {
                Buttons = new ReactiveProperty<ContextMenuButton[]>(new[]
                {
                    new ContextMenuButton
                    {
                        Text = "Place",
                        Icon = "MoveIcon",
                        Action = () => InteractionManager.Instance.HandlePlaceToggle()
                    },
                    new ContextMenuButton
                    {
                        Text = "Scale",
                        Icon = "ExpandIcon",
                        Action = () => InteractionManager.Instance.HandleDragToScale()
                    },
                    new ContextMenuButton
                    {
                        Text = "Rotate",
                        Icon = "RotateIcon",
                        Action = () => InteractionManager.Instance.HandleDragToRotate()
                    },
                    new ContextMenuButton
                    {
                        Text = "Menu",
                        Icon = "MenuIcon",
                        Action = () => InteractionManager.Instance.HandleShowProjectMenu()
                    }
                }),
                IsActive = new ReactiveProperty<bool>(false)
            },
            ProjectMenu = new ProjectMenu
            {
                IsActive = new BoolReactiveProperty(true),
                IsTagalong = new ReactiveProperty<bool>(true)
            },
            ForestManipulationMode = new ReactiveProperty<ForestManipulationMode>(ForestManipulationMode.DragToScale),
            IsPlacing = new ReactiveProperty<bool>(false),
            AvailableExampleProjects = new[] {"Sunflower", "CirclePacking", "Air", "Dcom", "Fupo"},
            Forest = new ReactiveProperty<Forest>(new Forest()),
            AppData = new ReactiveProperty<AppData>(new AppData())
        };

        private void Start()
        {
            AppState.AppData.Subscribe(data =>
            {
                SceneManipulator.Instance.DestroyForest();

                AppState.Forest.Value = new Forest
                {
                    Root = AppToUiMapper.Map(data.Root)
                };

                StartCoroutine(Render(AppState.Forest.Value.Root));
            });
        }

        private IEnumerator GetAppDataFromSonarQube(string projectName, string sonarQubeBaseComponent)
        {
            var sonarQubeCoroutine =
                this.StartCoroutine<SoftwareArtefact>(SonarQubeService.Instance.GetSoftwareArtefact(
                    sonarQubeBaseComponent,
                    projectName));
            yield return sonarQubeCoroutine.coroutine;
            yield return SoftwareArtefactToNodeMapper.Map(sonarQubeCoroutine.value);
        }

        private static IEnumerator GetAppDataFromFile(string projectName)
        {
            var softwareRoot = StreamingAssetsService.Instance.DesirializeData<Package>(projectName);
            yield return SoftwareArtefactToNodeMapper.Map(softwareRoot);
        }

        private static IEnumerator Render(UiNode root)
        {
            var innerNode = root as UiInnerNode;
            if (innerNode == null || innerNode.Children?.Count == 0) yield break;
            innerNode.SortChildren();
            var trees = innerNode.Children;
            yield return null;

            for (var i = 0; i < trees.Count; i++)
            {
                var treeObject = TreeBuilder.Instance.GenerateTree(trees[i], CalcTreePosition(trees, i));
                trees[i].Circle.Position.Subscribe(v =>
                {
                    treeObject.transform.localPosition = new Vector3(v.x, 0, v.y);
                });
                yield return null;
            }

            TreeBuilder.Instance.CirclePacking(innerNode);
            SceneManipulator.Instance.AdjustFloorRadius(innerNode);
        }

        private static Vector2 CalcTreePosition(IReadOnlyCollection<UiNode> trees, int n)
        {
            const float rowDistance = 1.3f;
            var forestLength = (int) Math.Floor(Math.Sqrt(trees.Count));
            var x = (n % forestLength - forestLength / 2) * rowDistance;
            var y = (n / forestLength - forestLength / 2) * rowDistance;
            return new Vector2(x, y);
        }
    }
}