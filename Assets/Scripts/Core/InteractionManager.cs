using System.Linq;
using Data;
using Frontend;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine;
using Utilities;

namespace Core
{
    public class InteractionManager : Singleton<InteractionManager>
    {
        private AppState AppState;

        public void Start()
        {
            AppState = ApplicationManager.Instance.AppState;
        }

        public void HandleNodeClick(string id)
        {
            var selected = FindUiNode(id);
            if (selected == null) return;
            selected.IsSelected.Value ^= true;
        }

        public void HandleNodeFocusEnter(string id)
        {
            var focused = FindUiNode(id);
            focused?
                .Traverse(x => (x as UiInnerNode)?.Children)
                .ToList()
                .ForEach(x => x.IsFocused.Value = true);
        }

        public void HandleNodeFocusExit(string id)
        {
            var focused = FindUiNode(id);
            focused?
                .Traverse(x => (x as UiInnerNode)?.Children)
                .ToList()
                .ForEach(x => x.IsFocused.Value = false);
        }

        public void HandleEmptyClick()
        {
            if (GazeManager.Instance.HitObject != null &&
                !GazeManager.Instance.HitObject.transform.IsChildOf(SpatialMappingManager.Instance.transform)) return;
//            AppState.Forest.Value?.Root?
//                .Traverse(x => (x as UiInnerNode)?.Children)
//                .ToList()
//                .ForEach(x => x.IsSelected.Value = false);
            HandleFloorClick();
        }

        private UiNode FindUiNode(string id)
        {
            return AppState.Forest.Value?.Root?
                .Traverse(x => (x as UiInnerNode)?.Children)
                .FirstOrDefault(x => x.Id == id);
        }

        public void HandleProjectSelection(int index)
        {
            var fileName = AppState.AvailableExampleProjects[index];
            var softwareRoot = StreamingAssetsService.Instance.DesirializeData<Package>(fileName);
            AppState.AppData.Value = new AppData {Root = SoftwareArtefactToNodeMapper.Map(softwareRoot)};
        }

        public void HandleFloorClick()
        {
            AppState.ContexMenu.IsActive.Value ^= true;
        }

        public void HandleIsPlacingToggle()
        {
            AppState.ContexMenu.IsActive.Value = false;
            AppState.IsPlacing.Value ^= true;
        }

        public void HandleScaleMode()
        {
            AppState.ContexMenu.IsActive.Value = false;
            AppState.ForestManipulationMode.Value = ManipulationMode.Scale;
            AppState.ManipulationIndicators.Mode.Value = ManipulationMode.Scale;
        }

        public void HandleRotateMode()
        {
            AppState.ContexMenu.IsActive.Value = false;
            AppState.ForestManipulationMode.Value = ManipulationMode.Rotate;
            AppState.ManipulationIndicators.Mode.Value = ManipulationMode.Rotate;
        }

        public void HandleShowProjectMenu()
        {
            AppState.ContexMenu.IsActive.Value = false;
            AppState.ProjectMenu.IsActive.Value = true;
            AppState.ProjectMenu.IsTagalong.Value = true;
        }

        public void HandleProjectMenuTagalongToggle()
        {
            AppState.ProjectMenu.IsTagalong.Value ^= true;
        }

        public void HandleProjectMenuHandDrag()
        {
            AppState.ProjectMenu.IsTagalong.Value = false;
        }
        
        public void HandleManipulationStarted()
        {
            AppState.ManipulationIndicators.IsActive.Value = true;
        }
        
        public void HandleManipulationCompleted()
        {
            AppState.ManipulationIndicators.IsActive.Value = false;
        }
    }
}