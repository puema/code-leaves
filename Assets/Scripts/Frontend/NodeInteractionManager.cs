using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class NodeInteractionManager : Singleton<NodeInteractionManager>, IInputClickHandler {

	private readonly List<GameObject> selectedNodes = new List<GameObject>();
	
	public void Start () {
		InputManager.Instance.PushFallbackInputHandler(gameObject);
	}
	
	public void OnInputClicked(InputClickedEventData eventData)
	{
		foreach (var node in selectedNodes)
		{
			var label = NodeInputHandler.GetLabelObject(node);
			if (label != null)
			{
				label.SetActive(false);
			}
		}
	}
	
	public void AddSelectedNode(GameObject node)
	{
		selectedNodes.Add(node);
	}
	
	public void RemoveSelectedNode(GameObject node)
	{
		selectedNodes.Remove(node);
	}
}
