using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class NodeInputHandler : MonoBehaviour, IInputClickHandler, IFocusable
{
    public void OnInputClicked(InputClickedEventData eventData)
    {
        TreeBuilder.Instance.HandleClickSomehow(GetComponent<ID>().Id);
        
//        var labelObject = GetLabelObject(gameObject);
//        if (labelObject == null) return;
//        if (labelObject.activeSelf)
//        {
//            labelObject.SetActive(false);
//            NodeInteractionManager.Instance.RemoveSelectedNode(gameObject);
//        }
//        else
//        {
//            labelObject.SetActive(true);
//            NodeInteractionManager.Instance.AddSelectedNode(gameObject);
//        }
    }

    public void OnFocusEnter()
    {
        GetComponent<MeshRenderer>().material.shader = Shader.Find("Outlined/Diffuse");
    }

    public void OnFocusExit()
    {
        GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
    }

    public static void ToggleActive(GameObject obj)
    {
        obj.SetActive(!obj.activeSelf);
    }

    public static GameObject GetLabelObject(GameObject obj)
    {
        var labelTransform = obj.transform.Find(TreeBuilder.LabelName) ??
                             obj.transform.parent.Find(TreeBuilder.NodeName).Find(TreeBuilder.LabelName);
        if (labelTransform != null) return labelTransform.gameObject;
        Debug.Log("No label found!");
        return null;
    }
}