using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.InputHandler
{
    public class CircleInputHandler : MonoBehaviour, IFocusable
    {
        public void OnFocusEnter()
        {
            InteractionManager.Instance.HandleNodeFocusEnter(GetNodeId());
        }

        public void OnFocusExit()
        {
            InteractionManager.Instance.HandleNodeFocusExit(GetNodeId());
        }
    
        public string GetNodeId()
        {
            // Up the hierarchy: circle -> node -> branch => node we are searching for
            var component = transform.parent.parent.parent.GetComponent<ID>();
            if (component != null) return component.Id;
            Debug.Log("Node ID not found!");
            return null;
        }
    }
}