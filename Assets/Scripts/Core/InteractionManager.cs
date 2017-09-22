using System.Linq;
using Data;
using Frontend;
using HoloToolkit.Unity;
using UnityEngine;
using Utilities;
using Logger = Utilities.Logger;

namespace Core
{
    public class InteractionManager : Singleton<InteractionManager>
    {
        private AppState AppState;

        public void Start()
        {
            AppState = ApplicationManager.Instance.AppState;
        }

        public void HandleFloorMenuInput(FloorInteractionMode mode)
        {
            AppState.FloorInteractionMode.Value = mode;
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
            if (focused == null) return;
            focused.IsFocused.Value = true;
        }

        public void HandleNodeFocusExit(string id)
        {
            var focused = FindUiNode(id);
            if (focused == null) return;
            focused.IsFocused.Value = false;
        }

        public void HandleFloorInteractionCompleted()
        {
            AppState.FloorInteractionMode.Value = FloorInteractionMode.TapToMenu;
        }

        public void HandleEmptyClick()
        {
            AppState.Forest.Value?.Root?
                .Traverse(x => (x as UiInnerNode)?.Children)
                .ToList()
                .ForEach(x => x.IsSelected.Value = false);
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
    }
}