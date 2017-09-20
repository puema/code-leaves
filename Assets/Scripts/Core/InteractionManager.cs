using System.Linq;
using Frontend;
using HoloToolkit.Unity;
using UniRx;
using UnityEngine;
using Utilities;

namespace Core
{
    public class InteractionManager : Singleton<InteractionManager>
    {
        private AppState AppState;
        private ReactiveProperty<Forest> Forest;

        public void Start()
        {
            AppState = ApplicationManager.Instance.AppState;
            Forest = ApplicationManager.Instance.Forest;
        }

        public void HandleMenuInput(FloorInteractionMode mode)
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
            Forest.Value.Root
                .Traverse(x => (x as UiInnerNode)?.Children)
                .ToList()
                .ForEach(x => x.IsSelected.Value = false);
        }

        private UiNode FindUiNode(string id)
        {
            return Forest.Value.Root
                .Traverse(x => (x as UiInnerNode)?.Children)
                .FirstOrDefault(x => x.Id == id);
        }
    }
}