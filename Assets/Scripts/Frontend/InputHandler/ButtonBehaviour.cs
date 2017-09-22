using System.Linq;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.InputHandler
{
    public class ButtonBehaviour : MonoBehaviour, IFocusable, IInputHandler
    {
        public Material HighlightMaterial;
        public GameObject HighlightTarget;
        public bool AnimateClick;
        public float ClickOffset;

        private Material[] OriginalMaterials;
        private float OriginalZ;

        public void OnFocusEnter()
        {
            OriginalMaterials = HighlightTarget.GetComponent<MeshRenderer>().materials;
            OriginalZ = transform.localPosition.z;
            HighlightTarget.GetComponent<MeshRenderer>().materials =
                OriginalMaterials.Concat(new[] {HighlightMaterial}).ToArray();
        }

        public void OnFocusExit()
        {
            HighlightTarget.GetComponent<MeshRenderer>().materials = OriginalMaterials;
        }

        public void OnInputUp(InputEventData eventData)
        {
            if (!AnimateClick) return;
            transform.localPosition += Vector3.back * ClickOffset;
        }

        public void OnInputDown(InputEventData eventData)
        {
            if (!AnimateClick) return;
            transform.localPosition += Vector3.forward * ClickOffset;
        }
        
    }
}