using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class NodeInputHandler : MonoBehaviour, IInputClickHandler, IFocusable
{
    public void OnInputClicked(InputClickedEventData eventData)
    {
        InteractionManager.Instance.HandleNodeClick(GetNodeId(gameObject));
    }

    public void OnFocusEnter()
    {
        InteractionManager.Instance.HandleNodeFocusEnter(GetNodeId(gameObject));
    }

    public void OnFocusExit()
    {
        InteractionManager.Instance.HandleNodeFocusExit(GetNodeId(gameObject));
    }
    
    public static string GetNodeId(GameObject obj)
    {
        var component = obj.GetComponent<ID>() ??
                        obj.transform.parent.Find(TreeBuilder.NodeName).GetComponent<ID>();
        if (component != null) return component.Id;
        Debug.Log("Node ID not found!");
        return null;
    }
}