﻿using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.Global
{
    public class FallbackInputHandler : MonoBehaviour, IInputClickHandler
    {
        private void Start()
        {
            InputManager.Instance.PushFallbackInputHandler(gameObject);
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleEmptyClick(); 
        }
    }
}