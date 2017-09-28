using System.Linq;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.Global
{
    public class ButtonBehaviour : MonoBehaviour, IFocusable, IInputHandler, ISourceStateHandler, IInputClickHandler
    {
        public Material HighlightMaterial;
        public GameObject HighlightTarget;
        public bool BackgroundOnFocusOnly;
        public bool AnimateClick;
        public float ClickOffset;

        private Material[] OriginalMaterials;
        private float OriginalZ;

        private void Start()
        {
            OriginalZ = transform.localPosition.z;
            OriginalMaterials = HighlightTarget.GetComponent<MeshRenderer>().materials;

            if (BackgroundOnFocusOnly)
            {
                SetBackgroundActive(false);
            }
        }

        private void SetBackgroundActive(bool isActive)
        {
            HighlightTarget.GetComponent<MeshRenderer>().enabled = isActive;
        }

        public void OnFocusEnter()
        {
            if (BackgroundOnFocusOnly)
            {
                SetBackgroundActive(true);
            }
            
            HighlightTarget.GetComponent<MeshRenderer>().materials =
                OriginalMaterials.Concat(new[] {HighlightMaterial}).ToArray();
            
            if (!AnimateClick) return;
            transform.localPosition += Vector3.back * ClickOffset;
        }

        public void OnFocusExit()
        {
            if (BackgroundOnFocusOnly)
            {
                SetBackgroundActive(false);
            }
            
            ResetMaterials();
            
            if (!AnimateClick) return;
            
            ResetPosition();
        }

        public void OnInputUp(InputEventData eventData)
        {
            if (!AnimateClick) return;
            transform.localPosition += Vector3.back * ClickOffset;
        }

        public void OnInputDown(InputEventData eventData)
        {
            if (!AnimateClick) return;
            ResetPosition();
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

        public void OnInputClicked(InputClickedEventData eventData)
        {
            ResetMaterials();
            ResetMaterials();
        }
    }
}