using System;
using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend
{
    public class DragToRotate : MonoBehaviour, IManipulationHandler, IHoldHandler
    {
        public GameObject Target;
        public ManipulationIndicators ManipulationIndicators;

        public float RotateFactor = 100;

        private float OriginalRotation;
        private Camera mainCamera;

        private void Start()
        {
            if (Target == null) Target = gameObject;
            mainCamera = Camera.main;
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
            ManipulationIndicators.Position();
            ManipulationIndicators.ActivateHand();
            ManipulationIndicators.ActivateIndicators();
            OriginalRotation = Target.transform.localEulerAngles.y;
            InputManager.Instance.PushModalInputHandler(gameObject);
            OnManipulationUpdated(eventData);
        }

        public void OnManipulationUpdated(ManipulationEventData eventData)
        {
            var direction = mainCamera.transform.InverseTransformDirection(eventData.CumulativeDelta);
            ManipulationIndicators.UpdateHandPosition(new Vector3(direction.x, 0, 0));
            Rotate(direction.x);
        }

        public void OnManipulationCompleted(ManipulationEventData eventData)
        {
            ManipulationIndicators.Deactivate();
            InputManager.Instance.PopModalInputHandler();
        }

        public void OnManipulationCanceled(ManipulationEventData eventData)
        {
            ManipulationIndicators.Deactivate();
            ApplyRotation(OriginalRotation);
        }

        private void Rotate(float delta)
        {
            var rotation = OriginalRotation - delta * RotateFactor;
            ApplyRotation(rotation);
        }

        private void ApplyRotation(float rotation)
        {
            Target.transform.localEulerAngles = Vector3.up * rotation;
        }
    }
}