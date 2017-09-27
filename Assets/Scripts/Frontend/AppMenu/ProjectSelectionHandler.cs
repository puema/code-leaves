using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.AppMenu
{
    
    public class ProjectSelectionHandler : MonoBehaviour, IInputClickHandler
    {
        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleProjectSelection(transform.GetSiblingIndex());
        }
    }
}