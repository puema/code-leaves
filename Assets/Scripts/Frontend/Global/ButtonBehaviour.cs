using System.Linq;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.Global
{
    public class ButtonBehaviour : MonoBehaviour, IFocusable, IInputHandler, ISourceStateHandler
    {
        public Material HighlightMaterial;
        public GameObject HighlightTarget;
        public bool AnimateClick;
        public float ClickOffset;

        private Material[] OriginalMaterials;
        private float OriginalZ;

        private void Start()
        {
            OriginalZ = transform.localPosition.z;
            OriginalMaterials = HighlightTarget.GetComponent<MeshRenderer>().materials;
        }

        public void OnFocusEnter()
        {
            HighlightTarget.GetComponent<MeshRenderer>().materials =
                OriginalMaterials.Concat(new[] {HighlightMaterial}).ToArray();
        }

        public void OnFocusExit()
        {
            ResetMaterials();
            if (!AnimateClick) return;
            ResetPosition();
        }

        public void OnInputUp(InputEventData eventData)
        {
            if (!AnimateClick) return;
            ResetPosition();
        }

        public void OnInputDown(InputEventData eventData)
        {
            if (!AnimateClick) return;
            transform.localPosition += Vector3.forward * ClickOffset;
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            ResetMaterials();
            ResetPosition();
        }

        private void ResetPosition()
        {
            var x = transform.localPosition.x;
            var y = transform.localPosition.y;
            var z = OriginalZ;
            transform.localPosition = new Vector3(x, y, z);
        }

        private void ResetMaterials()
        {
            HighlightTarget.GetComponent<MeshRenderer>().materials = OriginalMaterials;
        }
    }
}