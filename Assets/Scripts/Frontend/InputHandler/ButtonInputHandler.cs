using System.Linq;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.InputHandler
{
    public class ButtonInputHandler : MonoBehaviour, IInputClickHandler, IFocusable
    {
        public Material HighlightMaterial;
        public GameObject HighlightTarget;

        private Material[] originalMaterials;

        public void OnInputClicked(InputClickedEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnFocusEnter()
        {
            originalMaterials = HighlightTarget.GetComponent<MeshRenderer>().materials;
            HighlightTarget.GetComponent<MeshRenderer>().materials =
                originalMaterials.Concat(new[] {HighlightMaterial}).ToArray();
        }

        public void OnFocusExit()
        {
            HighlightTarget.GetComponent<MeshRenderer>().materials = originalMaterials;
        }
    }
}