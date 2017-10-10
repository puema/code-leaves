using Core;
using Frontend.Global;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UniRx;

namespace Frontend.AppMenu
{
    public class HighlightFocusedInputHandler : MonoBehaviour, IInputClickHandler
    {
        public ApplicationManager AppManager;

        private ReactiveProperty<bool> highlightFocused;
        
        private void Start()
        {
            highlightFocused = AppManager.AppState.Settings.HighlightFocused;
            highlightFocused.Subscribe(GetComponentInChildren<Checkbox>().SetChecked);
            GetComponentInChildren<ButtonBehaviour>().Clicked +=
                InteractionManager.Instance.HandleHighlightFocusToggle;
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleHighlightFocusToggle();
        }
    }
}