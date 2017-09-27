using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.Forest
{
    public class FloorInputHandler : MonoBehaviour, IInputClickHandler
    {
        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleFloorClick();
        }
    }
}