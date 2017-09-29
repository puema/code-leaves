using Data;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;

namespace Core
{
    public class InteractionManager : Singleton<InteractionManager>
    {
        private AppState appState;
        private UiElements uiElements;

        public void Start()
        {
            appState = ApplicationManager.Instance.AppState;
            uiElements = appState.UiElements;
        }

        public void HandleNodeClick(string id)
        {
            var selected = appState.Forest.Value.Root.Find(id);
            if (selected == null) return;
            selected.IsSelected.Value ^= true;
        }

        public void HandleNodeFocusEnter(string id)
        {
            NodeFocusManager.Instance.HandleFocusEnter(id);
        }

        public void HandleNodeFocusExit(string id)
        {
            NodeFocusManager.Instance.HandleFocusExit(id);
        }

        public void HandleCircleVisualizationToggle()
        {
            appState.Settings.VisualizeCircles.Value ^= true;
        }

        public void HandleEmptyClick()
        {
            if (GazeManager.Instance.HitObject != null &&
                !GazeManager.Instance.HitObject.transform.IsChildOf(SpatialMappingManager.Instance.transform)) return;
            HandleFloorClick();
        }

        public void HandleProjectSelection(int index)
        {
            var fileName = appState.AvailableExampleProjects[index];
            var softwareRoot = StreamingAssetsService.Instance.DesirializeData<Package>(fileName);
            appState.AppData.Value = new AppData {Root = SoftwareArtefactToNodeMapper.Map(softwareRoot)};
            appState.UiElements.AppMenu.Page.Value = AppMenuPage.Settings;
            uiElements.AppMenu.BackAvailable.Value = true;
        }

        public void HandleFloorClick()
        {
            uiElements.ContexMenu.IsActive.Value ^= true;
        }

        public void HandleIsPlacingToggle()
        {
            uiElements.ContexMenu.IsActive.Value = false;
            uiElements.IsPlacing.Value ^= true;
        }

        public void HandleScaleMode()
        {
            uiElements.ContexMenu.IsActive.Value = false;
            uiElements.ForestManipulationMode.Value = ManipulationMode.Scale;
            uiElements.ManipulationIndicators.Mode.Value = ManipulationMode.Scale;
        }

        public void HandleRotateMode()
        {
            uiElements.ContexMenu.IsActive.Value = false;
            uiElements.ForestManipulationMode.Value = ManipulationMode.Rotate;
            uiElements.ManipulationIndicators.Mode.Value = ManipulationMode.Rotate;
        }

        public void HandleShowProjectMenu()
        {
            uiElements.ContexMenu.IsActive.Value = false;
            uiElements.AppMenu.IsActive.Value = true;
            uiElements.AppMenu.IsTagalong.Value = true;
        }

        public void HandleAppMenuTagalongToggle()
        {
            uiElements.AppMenu.IsTagalong.Value ^= true;
        }

        public void HandleAppMenuHandDrag()
        {
            uiElements.AppMenu.IsTagalong.Value = false;
        }
        
        public void HandleManipulationStarted()
        {
            uiElements.ManipulationIndicators.IsActive.Value = true;
        }
        
        public void HandleManipulationCompleted()
        {
            uiElements.ManipulationIndicators.IsActive.Value = false;
        }

        public void HandleAppMenuBack()
        {
            if (uiElements.AppMenu.Page.Value != AppMenuPage.Settings) return;
            
            uiElements.AppMenu.Page.Value = AppMenuPage.ProjectSelection;
            uiElements.AppMenu.BackAvailable.Value = false;
        }

        public void HandleHighlightFocusToggle()
        {
            appState.Settings.HighlightFocused.Value ^= true;
        }
    }
}