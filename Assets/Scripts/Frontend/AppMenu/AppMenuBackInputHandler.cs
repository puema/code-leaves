using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.AppMenu
{
    public class AppMenuBackInputHandler : MonoBehaviour, IInputClickHandler
    {
        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleAppMenuBack();
        }
    }
}