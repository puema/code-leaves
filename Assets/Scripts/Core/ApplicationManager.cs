using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Frontend;
using Frontend.Forest;
using Frontend.Models;
using Frontend.Tree;
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
            UiElements = new UiElements
            {
                GazeText = new GazeText
                {
                    IsActive = new BoolReactiveProperty(false),
                    Text = new ReactiveProperty<string>()
                },
                ContexMenu = new ContextMenu
                {
                    Buttons = new ReactiveProperty<ContextMenuButton[]>(new[]
                    {
                        new ContextMenuButton
                        {
                            Text = "Place",
                            Icon = "hand",
                            Action = () => InteractionManager.Instance.HandleIsPlacingToggle()
                        },
                        new ContextMenuButton
                        {
                            Text = "Scale",
                            Icon = "expand",
                            Action = () => InteractionManager.Instance.HandleScaleMode()
                        },
                        new ContextMenuButton
                        {
                            Text = "Rotate",
                            Icon = "rotate",
                            Action = () => InteractionManager.Instance.HandleRotateMode()
                        },
                        new ContextMenuButton
                        {
                            Text = "Menu",
                            Icon = "menu",
                            Action = () => InteractionManager.Instance.HandleShowProjectMenu()
                        }
                    }),
                    IsActive = new ReactiveProperty<bool>(false)
                },
                AppMenu = new ProjectMenu
                {
                    IsActive = new BoolReactiveProperty(true),
                    IsTagalong = new ReactiveProperty<bool>(true)
                },
                ManipulationIndicators = new ManipulationIndicators
                {
                    IsActive = new ReactiveProperty<bool>(false),
                    Mode = new ReactiveProperty<ManipulationMode>()
                },
                ForestManipulationMode = new ReactiveProperty<ManipulationMode>(ManipulationMode.Scale),
                IsPlacing = new ReactiveProperty<bool>(false)
            },
            AvailableExampleProjects = new[]
            {
                "AirExcerpt", "AirTools", "AirCps", "Air"
            },
            Forest = new ReactiveProperty<Forest>(new Forest()),
            AppData = new ReactiveProperty<AppData>(new AppData())
        };

        private void Start()
        {
            AppState.AppData.Subscribe(data =>
            {
                ForestManipulator.Instance.DestroyForest();

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
            ForestManipulator.Instance.AdjustFloorRadius(innerNode);
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