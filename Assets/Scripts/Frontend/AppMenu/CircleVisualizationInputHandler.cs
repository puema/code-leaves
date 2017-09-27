using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.AppMenu
{
    public class CircleVisualizationInputHandler : MonoBehaviour, IInputClickHandler
    {
        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleCircleVisualizationToggle();
        }
    }
}