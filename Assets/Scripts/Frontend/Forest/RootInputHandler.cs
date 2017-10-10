using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.Forest
{
    public class RootInputHandler : MonoBehaviour, IInputClickHandler
    {
        public void OnInputClicked(InputClickedEventData eventData)
        {
            Debug.Log(transform.GetSiblingIndex());
        }   
    }
}