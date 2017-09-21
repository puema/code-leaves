using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend
{
    public class MenuInputHandler : MonoBehaviour, IInputClickHandler
    {
        public FloorInteractionMode Mode;

        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleMenuInput(Mode);
        }
    }
}