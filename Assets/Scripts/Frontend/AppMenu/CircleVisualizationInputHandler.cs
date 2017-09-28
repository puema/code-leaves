using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UniRx;

namespace Frontend.AppMenu
{
    public class CircleVisualizationInputHandler : MonoBehaviour, IInputClickHandler
    {
        public ApplicationManager AppManager;

        private ReactiveProperty<bool> visualizeCircles;
        
        private void Start()
        {
            visualizeCircles = AppManager.AppState.Settings.VisualizeCircles;
            visualizeCircles.Subscribe(GetComponentInChildren<Checkbox>().SetChecked);
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleCircleVisualizationToggle();
        }
    }
}