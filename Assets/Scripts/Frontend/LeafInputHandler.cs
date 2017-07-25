using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class LeafInputHandler : MonoBehaviour, IInputClickHandler
{
    public void OnInputClicked(InputClickedEventData eventData)
    {
        var color = TreeBuilder.HexToNullableColor("#208000");
        GetComponent<MeshRenderer>().material.color = color ?? new Color();
    }
}
