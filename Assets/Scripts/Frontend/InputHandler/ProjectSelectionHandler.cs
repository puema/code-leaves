using System;
using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.InputHandler
{
    
    public class ProjectSelectionHandler : MonoBehaviour, IInputClickHandler
    {
        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleProjectSelection(transform.GetSiblingIndex());
        }
    }
}