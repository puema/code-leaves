using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.InputHandler
{
    public class FloorInputHandler : MonoBehaviour, IInputClickHandler
    {
        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleFloorInput();
        }
    }
}