using System;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.Global
{
    public class ContextButtonInputHandler : MonoBehaviour, IInputClickHandler
    {
        public Action Action;
        
        public void OnInputClicked(InputClickedEventData eventData)
        {
            Action.Invoke();
        }
    }
}