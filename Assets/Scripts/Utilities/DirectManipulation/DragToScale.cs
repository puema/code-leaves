using System;
using Core;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend
{
    public class DragToScale : MonoBehaviour, IManipulationHandler, IHoldHandler
    {
        public GameObject Target;
        public ManipulationIndicators ManipulationIndicators;
        
        public float ScaleFactor = 1;
        public float MinSize = 0.5f;
        public float MinScale = 0.01f;
        public float MaxScale = 10;

        private float OriginalScale;
        private Camera mainCamera;

        private void Start()
        {
            if (Target == null) Target = gameObject;
            mainCamera = Camera.main;
        }

        private void SetMinMaxScale()
        {
            MinScale = TreeGeometry.SizeToScale(MinSize, GetComponentInChildren<Renderer>().bounds.size.x,
                gameObject.transform.localScale.x);
        }
        
        public void OnHoldStarted(HoldEventData eventData)
        {
            ManipulationIndicators.Position();
            ManipulationIndicators.ActivateIndicators();
        }

        public void OnHoldCompleted(HoldEventData eventData)
        {
            ManipulationIndicators.Deactivate();
        }

        public void OnHoldCanceled(HoldEventData eventData)
        {
            ManipulationIndicators.Deactivate();
        }

        public void OnManipulationStarted(ManipulationEventData eventData)
        {
            SetMinMaxScale();
            ManipulationIndicators.Position();
            ManipulationIndicators.ActivateHand();
            ManipulationIndicators.ActivateIndicators();
            OriginalScale = Target.transform.localScale.x;
            InputManager.Instance.PushModalInputHandler(gameObject);
            OnManipulationUpdated(eventData);
        }

        public void OnManipulationUpdated(ManipulationEventData eventData)
        {
            var direction = mainCamera.transform.InverseTransformDirection(eventData.CumulativeDelta);
            ManipulationIndicators.UpdateHandPosition(new Vector3(direction.x, 0, 0));
            Scale(direction.x);
        }

        public void OnManipulationCompleted(ManipulationEventData eventData)
        {
            ManipulationIndicators.Deactivate();
            InputManager.Instance.PopModalInputHandler();
        }

        public void OnManipulationCanceled(ManipulationEventData eventData)
        {
            ManipulationIndicators.Deactivate();
            ApplyScale(OriginalScale);
        }

        private void Scale(float delta)
        {
            var scale = Mathf.Clamp(OriginalScale + delta * ScaleFactor, MinScale, MaxScale);
            ApplyScale(scale);
        }

        private void ApplyScale(float scale)
        {
            Target.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}