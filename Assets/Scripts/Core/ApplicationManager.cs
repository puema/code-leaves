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
                AppMenu = new AppMenu
                {
                    IsActive = new BoolReactiveProperty(true),
                    IsTagalong = new ReactiveProperty<bool>(true),
                    Page = new ReactiveProperty<AppMenuPage>(AppMenuPage.ProjectSelection),
                    BackAvailable = new ReactiveProperty<bool>(false)
                },
                ManipulationIndicators = new ManipulationIndicators
                {
                    IsActive = new ReactiveProperty<bool>(false),
                    Mode = new ReactiveProperty<ManipulationMode>()
                },
                ForestManipulationMode = new ReactiveProperty<ManipulationMode>(ManipulationMode.Scale),
                IsPlacing = new ReactiveProperty<bool>(false)
            },
            Settings = new Settings
            {
                VisualizeCircles = new ReactiveProperty<bool>(false),
                HighlightFocused = new ReactiveProperty<bool>(true)
            },
            AvailableExampleProjects = new[]
            {
                "AirExcerpt", "AirCbs", "AirTools", "Air"
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

        private static IEnumerator Render(UiNode root)
        {
            var innerNode = root as UiInnerNode;
            if (innerNode == null || innerNode.Children?.Count == 0) yield break;
            innerNode.SortChildren();
            var trees = innerNode.Children;
            yield return null;

            foreach (var tree in trees)
            {
                var treeObject = TreeBuilder.Instance.GenerateTree(tree, Vector2.zero);
                tree.Circle.Position.Subscribe(v =>
                {
                    treeObject.transform.localPosition = new Vector3(v.x, 0, v.y);
                });
                yield return null;
            }

            TreeBuilder.Instance.CirclePacking(innerNode);
            ForestManipulator.Instance.AdjustFloorRadius(innerNode);
        }
    }
}