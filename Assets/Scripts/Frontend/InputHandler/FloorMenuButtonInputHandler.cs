using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.InputHandler
{
    public class FloorMenuButtonInputHandler : MonoBehaviour, IInputClickHandler
    {
        public FloorInteractionMode Mode;

        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleMenuInput(Mode);
        }
    }
}