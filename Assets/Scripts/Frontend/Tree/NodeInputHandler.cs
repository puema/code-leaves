using Core;
using Frontend.Forest;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.Tree
{
    public class NodeInputHandler : MonoBehaviour, IInputClickHandler, IFocusable
    {
        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleNodeClick(GetNodeId());
        }

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
            var component = gameObject.GetComponent<ID>() ??
                            gameObject.transform.parent.Find(ForestManipulator.NodeName).GetComponent<ID>();
            if (component != null) return component.Id;
            Debug.Log("Node ID not found!");
            return null;
        }
    }
}